using System.Collections;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
	[Header("Component References"), Space]
	[SerializeField] protected Rigidbody2D rb2D;
	[SerializeField] protected MonoBehaviour movementBehaviour;

	[Header("Damage Text"), Space]
	[SerializeField] protected GameObject dmgTextPrefab;
	[SerializeField] protected Transform dmgTextLoc;

	[Header("Base Stats"), Space]
	[SerializeField] protected int maxHealth;
	[SerializeField] protected float damageFlashTime;
	[SerializeField] protected float knockBackRes;

	// Protected fields.
	protected Material _mat;
	protected int _currentHealth;

	protected virtual void Start()
	{
		_currentHealth = maxHealth;
	}

	public virtual void TakeDamage(int amount, bool weakpointHit, Vector3 attackerPos = default, float knockBackStrength = 0f)
	{
		AudioManager.Instance.PlayWithRandomPitch("Taking Damage", .7f, 1.2f);
		
		_currentHealth -= amount;
		_currentHealth = Mathf.Max(0, _currentHealth);

		DamageTextStyle style = weakpointHit ? DamageTextStyle.Critical : DamageTextStyle.Normal;
		DamageText.Generate(dmgTextPrefab, dmgTextLoc.position, style, amount.ToString());

		EffectInstantiator.Instance.Instantiate<ParticleSystem>(EffectType.CreatureImpact, transform.position, transform.right);

		StartCoroutine(TriggerDamageFlash());
		StartCoroutine(BeingKnockedBack(attackerPos, knockBackStrength));

		if (_currentHealth <= 0)
			Die();
	}

	public virtual void Die()
	{
		EffectInstantiator.Instance.Instantiate<ParticleSystem>(EffectType.CreatureDeath, transform.position, Quaternion.identity);
	}

	protected IEnumerator TriggerDamageFlash()
	{
		float flashIntensity;
		float elapsedTime = 0f;

		while (elapsedTime < damageFlashTime)
		{
			elapsedTime += Time.deltaTime;

			flashIntensity = Mathf.Lerp(1f, 0f, elapsedTime / damageFlashTime);
			_mat.SetFloat("_FlashIntensity", flashIntensity);

			yield return null;
		}
	}

	protected IEnumerator BeingKnockedBack(Vector3 attackerPos, float strength)
	{
		if (attackerPos == default)
			yield break;

		rb2D.velocity = Vector3.zero;
		movementBehaviour.enabled = false;
		movementBehaviour.StopAllCoroutines();

		Vector2 direction = transform.position - attackerPos;
		float knockBackStrength = strength * (1f - knockBackRes);

		Vector2 force = direction.normalized * knockBackStrength;

		rb2D.AddForce(force, ForceMode2D.Impulse);

		yield return new WaitForSeconds(.3f);

		movementBehaviour.enabled = true;
	}
}
