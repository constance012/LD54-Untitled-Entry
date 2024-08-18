using UnityEngine;

[CreateAssetMenu(fileName = "New Ranged Weapon", menuName = "Inventory/Weapons/Ranged Weapon")]
public class RangedWeapon : Weapon
{
	public enum RangedWeaponType
	{
		Slingshot,
		Bow,
		Crossbow,
		Pistol,
		SMG,
		Rifle,
		Shotgun,
		Sniper,
		Heavy,
		Launcher
	}

	[Header("Ranged Weapon Type"), Space]
	public RangedWeaponType rangedWeaponType;

	[Header("Bullet Properties"), Space]
	public float verticalSpread = 0f;
	public int bulletsPerShot = 1;

	[Header("Ammo Properties"), Space]
	public bool infiniteAmmo;
	[Min(0)] public int reserveAmmo;

	[field: SerializeField, Min(0)]
	public int MagazineCapacity { get; private set; }

	[Min(0)] public int currentAmmo;

	[Header("Reload Properties"), Space]
	public float reloadTime;
	public bool CanReload => reserveAmmo > 0 && currentAmmo < MagazineCapacity;

	[HideInInspector] public bool isReloading;
	[HideInInspector] public bool promptReload;

	public const int LAYER_TO_RAYCAST = ~(1 << 6 | 1 << 9 | Physics2D.IgnoreRaycastLayer);

	public override string ToString()
	{
		return base.ToString() + "\n" +
				$"<b> Ammo: {currentAmmo} / {reserveAmmo} </b>";
	}

	#region Bullets Management.
	public override bool FireBullet(Vector2 rayOrigin, Vector2 rayDestination)
	{
		if (!CheckForAmmo())
			return false;

		SimpleRaycast(rayOrigin, rayDestination);

		if (!infiniteAmmo)
		{
			currentAmmo--;
			Inventory.Instance.UpdateItemTooltip(slotIndex);
		}

		return true;
	}

	private void SimpleRaycast(Vector2 start, Vector2 end)
	{
		Vector2 rayDirection = end - start;

		float spreadY = CalculateSpreading();
		rayDirection += new Vector2(0f, spreadY);

		Ray2D ray = new Ray2D(start, rayDirection);
		
		EffectInstantiator.Instance.Instantiate<ParticleSystem>(EffectType.MuzzleFlash, start, Quaternion.identity);
		
		TrailRenderer tracer = EffectInstantiator.Instance.Instantiate<TrailRenderer>(EffectType.BulletTracer, start, Quaternion.identity);
		tracer.AddPosition(start);

		bool hitSomething = Physics2DExtensions.Raycast(ray, out RaycastHit2D hitInfo, range, LAYER_TO_RAYCAST);

		if (hitSomething)
		{
			Debug.Log($"Hit {hitInfo.transform.name}");

			ProcessContact(hitInfo);

			tracer.transform.position = hitInfo.point;
		}
		else
			tracer.transform.position = end;
	}
	#endregion

	#region Bullet Reloading.
	public void StandardReload()
	{
		int firedBullets = MagazineCapacity - currentAmmo;

		if (reserveAmmo < firedBullets)
		{
			currentAmmo += reserveAmmo;
			reserveAmmo = 0;
		}
		else
		{
			currentAmmo += firedBullets;
			reserveAmmo -= firedBullets;
		}

		isReloading = false;
	}

	public void SingleRoundReload()
	{
		currentAmmo++;
		reserveAmmo--;
	}
	#endregion

	#region Private Methods.
	private bool CheckForAmmo()
	{
		if (currentAmmo == 0)
		{
			promptReload = true;
			return false;
		}

		if (isReloading)
			return false;

		return true;
	}

	private float CalculateSpreading()
	{
		float spreadY = Random.Range(-verticalSpread, verticalSpread);
		return spreadY;
	}

	private void ProcessContact(RaycastHit2D hitInfo)
	{
		if (hitInfo.collider.gameObject.layer == 8)
			EffectInstantiator.Instance.Instantiate<ParticleSystem>(EffectType.SolidImpact, hitInfo);
		
		if (hitInfo.rigidbody != null)
		{
			if (hitInfo.rigidbody.TryGetComponent(out EnemyStats enemy))
			{
				if (hitInfo.collider.CompareTag("WeakpointTrigger"))
				{
					int weakpointDamage = Mathf.FloorToInt(baseDamage * weakpointMultiplier);
					enemy.TakeDamage(weakpointDamage, true, PlayerController.Position, knockBackStrength);
				}
				else
					enemy.TakeDamage(baseDamage, false, PlayerController.Position, knockBackStrength);
			}
		}
	}
	#endregion
}