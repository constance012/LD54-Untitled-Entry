using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static Weapon;

public class PlayerActions : Singleton<PlayerActions>
{
	public static Weapon[] weapons = new Weapon[2];
	public static Electronic flashlight;

	[Header("Debugging: Range Visualization"), Space]
	[SerializeField] private float meleeRange;
	[SerializeField] private float shootingRange;

	[Header("Ranged Weapon"), Space]
	[SerializeField] private GameObject primaryWeapon;
	[SerializeField] private Transform firePoint;

	[Header("Melee Weapon"), Space]
	[SerializeField] private GameObject secondaryWeapon;
	[SerializeField] private Transform hitPoint;
	[SerializeField, Tooltip("How far would the melee hit point extent vertically?")]
	private float hitpointXExtent;

	[Header("Flashlight"), Space]
	[SerializeField] private GameObject flashlightGameObj;

	[Header("UI"), Space]
	[SerializeField] private TextMeshProUGUI weaponUIText;
	[SerializeField] private TextMeshProUGUI flashlightUIText;
	[SerializeField] private Image weaponIcon;

	[Space]
	[SerializeField] private Animator weaponAnimator;

	[Header("Enemy Layer"), Space]
	public LayerMask enemyLayer;

	private Weapon _currentWeapon;
	private float _timeForNextUse;

	private float _hitpointXOrigin;

	private void OnDisable()
	{
		CursorManager.Instance.SwitchCursorTexture(CursorTextureType.Default);
	}

	private void Start()
	{
		_hitpointXOrigin = hitPoint.localPosition.x;

		flashlightGameObj.SetActive(true);
		primaryWeapon.SetActive(false);
		secondaryWeapon.SetActive(false);

		weaponUIText.text = "UNARMED";
		weaponIcon.gameObject.SetActive(false);
	}

	private void Update()
	{
		if (GameManager.Instance.GameDone)
			return;

		ManageFlashlight();

		SelectWeapon();

		UseWeapon();
	}

	private void ManageFlashlight()
	{
		if (flashlight == null)
		{
			flashlightGameObj.SetActive(false);
			flashlightUIText.text = "DROPPED";
			return;
		}

		if (flashlight.IsTurnedOn)
		{
			if (flashlight.OutOfBattery)
				flashlightGameObj.SetActive(false);
			else
			{
				flashlight.UpdateBattery(Time.deltaTime);
				flashlightUIText.text = $"{flashlight.currentBatteryLife:0.0} %";
			}
		}

		if (InputManager.Instance.WasPressedThisFrame(KeybindingActions.Flashlight))
		{
			flashlight.IsTurnedOn = !flashlight.IsTurnedOn;
			
			flashlightUIText.text = flashlight.IsTurnedOn ? $"{flashlight.currentBatteryLife:0.0} %" : "OFF";
			flashlightGameObj.SetActive(flashlight.IsTurnedOn && !flashlight.OutOfBattery);
		}
	}

	private void SelectWeapon()
	{
		if (weapons.All(weapon => weapon == null))
			return;

		if (InputManager.Instance.WasPressedThisFrame(KeybindingActions.PrimaryWeapon))
			SwitchWeapon((int)WeaponSlot.Primary);

		if (InputManager.Instance.WasPressedThisFrame(KeybindingActions.SecondaryWeapon))
			SwitchWeapon((int)WeaponSlot.Secondary);
	}

	public void SwitchWeapon(int newWeaponIndex)
	{
		// If the new weapon is the same as the current one, switch to unarmed state.
		if (_currentWeapon == weapons[newWeaponIndex] || weapons[newWeaponIndex] == null)
		{
			CursorManager.Instance.SwitchCursorTexture(CursorTextureType.Default);
			
			primaryWeapon.SetActive(false);
			secondaryWeapon.SetActive(false);

			_currentWeapon = null;

			weaponIcon.gameObject.SetActive(false);
			weaponUIText.text = "UNARMED";
		}
		// Else, equip the new weapon.
		else
		{
			CursorManager.Instance.SwitchCursorTexture(CursorTextureType.Crosshair);

			_currentWeapon = weapons[newWeaponIndex];

			weaponIcon.gameObject.SetActive(true);
			weaponIcon.sprite = _currentWeapon.icon;

			if (_currentWeapon.type == WeaponType.Ranged)
			{
				RangedWeapon weapon = _currentWeapon as RangedWeapon;
				weaponUIText.text = $"<size=18>{weapon.currentAmmo} <size=12>/{weapon.reserveAmmo}";
			}
			else
				weaponUIText.text = _currentWeapon.itemName.ToUpper();

			primaryWeapon.SetActive(_currentWeapon.weaponSlot == WeaponSlot.Primary);
			secondaryWeapon.SetActive(_currentWeapon.weaponSlot == WeaponSlot.Secondary);
		}
	}

	private void UseWeapon()
	{
		if (_timeForNextUse > 0f)
			_timeForNextUse -= Time.deltaTime;

		if (_currentWeapon == null || Inventory.Instance.insideInventory)
			return;

		switch (_currentWeapon.type)
		{
			case WeaponType.Melee:
				float t = Mathf.Abs(Mathf.Sin(transform.eulerAngles.z * Mathf.Deg2Rad));
				
				float x = Mathf.Lerp(_hitpointXOrigin, hitpointXExtent, t);
				hitPoint.localPosition = new Vector3(x, 0f, 0f);

				if (InputManager.Instance.WasPressedThisFrame(KeybindingActions.Attack) && _timeForNextUse <= 0f)
				{
					AudioManager.Instance.Play("Knife Thrust");
					weaponAnimator.Play("Knife Thrust", 0, 0f);
					_timeForNextUse = _currentWeapon.useSpeed;
				}

				break;

			case WeaponType.Ranged:
				RangedWeapon weapon = _currentWeapon as RangedWeapon;

				if (!weapon.isReloading && (InputManager.Instance.WasPressedThisFrame(KeybindingActions.Reload) || weapon.promptReload))
					StartCoroutine(ReloadWeapon(weapon));

				if (InputManager.Instance.WasPressedThisFrame(KeybindingActions.Attack) && _timeForNextUse <= 0f)
				{
					ShootWeapon(weapon);
				}

				break;
		}
	}

	/// <summary>
	/// Method for animation event.
	/// </summary>
	public void KnifeThrust()
	{
		_currentWeapon.MeleeAttack(hitPoint.position, enemyLayer);
	}

	private void ShootWeapon(RangedWeapon weapon)
	{
		if (weapon.isReloading)
			return;

		Vector2 rayOrigin = firePoint.position;
		Vector2 rayDestination = Camera.main.ScreenToWorldPoint(Input.mousePosition);

		if (weapon.FireBullet(rayOrigin, rayDestination))
		{
			AudioManager.Instance.PlayWithRandomPitch("Gunshot", .7f, 1.2f);
			CameraShaker.Instance.ShakeCamera(2f, .15f);

			weaponUIText.text = $"<size=18>{weapon.currentAmmo} <size=12>/{weapon.reserveAmmo}";
			_timeForNextUse = weapon.useSpeed;
		}
	}

	private IEnumerator ReloadWeapon(RangedWeapon weapon)
	{
		weapon.promptReload = false;
		weapon.isReloading = true;

		if (!weapon.CanReload)
		{
			weapon.isReloading = false;
			yield break;
		}

		weaponUIText.text = "RELOADING...";

		yield return new WaitForSeconds(weapon.reloadTime);

		weapon.StandardReload();

		weaponUIText.text = $"<size=18>{weapon.currentAmmo} <size=12>/{weapon.reserveAmmo}";
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(hitPoint.position, meleeRange);

		Gizmos.color = Color.cyan;
		Gizmos.DrawRay(firePoint.position, firePoint.right * shootingRange);
	}
}