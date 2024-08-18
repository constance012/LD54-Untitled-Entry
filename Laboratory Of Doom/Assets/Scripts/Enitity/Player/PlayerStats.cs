using UnityEngine;

public class PlayerStats : Entity
{
	[Header("Player Stats"), Space]
	[SerializeField] private float invincibilityTime;

	public static bool IsDeath { get; set; }

	// Private fields.
	private float _invincibilityTime;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void ResetStatic()
	{
		IsDeath = false;
	}

	private void Awake()
	{
		_mat = this.GetComponentInChildren<SpriteRenderer>("Graphics/Player Sprite").material;
	}

	protected override void Start()
	{
		base.Start();

		GameManager.Instance.InitializeHealthBar(maxHealth);

		_invincibilityTime = invincibilityTime;
		IsDeath = false;
	}

	private void Update()
	{
		if (_invincibilityTime > 0f)
			_invincibilityTime -= Time.deltaTime;
	}

	public override void TakeDamage(int amount, bool weakpointHit, Vector3 attackerPos = default, float knockBackStrength = 0f)
	{
		if (_currentHealth > 0 && _invincibilityTime <= 0f)
		{
			base.TakeDamage(amount, weakpointHit, attackerPos, knockBackStrength);

			CameraShaker.Instance.ShakeCamera(2.5f, .15f);
			GameManager.Instance.UpdateCurrentHealth(_currentHealth);

			_invincibilityTime = invincibilityTime;
		}
	}

	public void Heal(int amount)
	{
		if (_currentHealth > 0)
		{
			_currentHealth += amount;
			_currentHealth = Mathf.Min(_currentHealth, maxHealth);

			GameManager.Instance.UpdateCurrentHealth(_currentHealth);

			DamageText.Generate(dmgTextPrefab, dmgTextLoc.position, DamageText.HealingColor, DamageTextStyle.Normal, amount.ToString());
		}
	}

	public override void Die()
	{
		base.Die();

		IsDeath = true;
		
		GameManager.Instance.ShowGameOverScreen();
		gameObject.SetActive(false);
	}
}
