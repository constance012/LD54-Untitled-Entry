using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Base Weapon", menuName = "Inventory/Weapons/Base Weapon")]
public class Weapon : Item
{
	public enum WeaponType { Melee, Ranged }
	public enum WeaponSlot { Primary, Secondary }

	[Header("Weapon Type"), Space]
	public WeaponType type;
	public WeaponSlot weaponSlot;

	[Header("Basic Stats"), Space]
	public int baseDamage;

	[Range(1f, 10f), Tooltip("The damage multiplier if the weapon hits the enemy's weakpoint.")]
	public float weakpointMultiplier;
	public float range;
	public float knockBackStrength;

	[Tooltip("An interval in seconds between each use.")]
	public float useSpeed;

	public virtual bool FireBullet(Vector2 rayOrigin, Vector2 rayDestination)
	{
		return false;
	}

	public virtual void MeleeAttack(Vector3 hitPoint, int layerMask)
	{
		Collider2D[] hitList = Physics2D.OverlapCircleAll(hitPoint, range, layerMask);
		HashSet<string> attackedEnemies = new HashSet<string>();

		foreach (Collider2D enemyCollider in hitList)
		{
			EnemyStats enemy = enemyCollider.GetComponentInParent<EnemyStats>();
			
			if (!enemyCollider.isTrigger || attackedEnemies.Contains(enemy.ID))
				continue;

			if (enemyCollider.CompareTag("WeakpointTrigger")) 
			{
				int weakpointDamage = Mathf.FloorToInt(baseDamage * weakpointMultiplier);
				enemy.TakeDamage(weakpointDamage, true, PlayerController.Position, knockBackStrength);
			}
			else
				enemy.TakeDamage(baseDamage, false, PlayerController.Position, knockBackStrength);

			attackedEnemies.Add(enemy.ID);
		}
	}

	public override string ToString()
	{
		return base.ToString() + "\n" +
				$"{weaponSlot} weapon.\n" +
				$"<b> Base Damage: {baseDamage} </b>";
	}
}
