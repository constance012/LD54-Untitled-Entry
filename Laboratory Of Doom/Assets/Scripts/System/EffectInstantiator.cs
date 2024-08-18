using UnityEngine;
using AYellowpaper.SerializedCollections;

public sealed class EffectInstantiator : Singleton<EffectInstantiator>
{
	[Header("Effect Prefabs"), Space]
	[SerializeField] private SerializedDictionary<EffectType, GameObject> effects = new SerializedDictionary<EffectType, GameObject>();

	#region Instantiate Method Overloads.
	public TEffect Instantiate<TEffect>(EffectType type)
								where TEffect : Component
	{
		if (effects.TryGetValue(type, out GameObject prefab))
		{
			GameObject effectObj = Instantiate(prefab);
			return effectObj.GetComponent<TEffect>();
		}

		return null;
	}

	public TEffect Instantiate<TEffect>(EffectType type, Transform parent)
								where TEffect : Component
	{
		if (effects.TryGetValue(type, out GameObject prefab))
		{
			GameObject effectObj = Instantiate(prefab, parent);
			return effectObj.GetComponent<TEffect>();
		}

		return null;
	}

	public TEffect Instantiate<TEffect>(EffectType type, Vector3 position, Quaternion rotation)
								where TEffect : Component
	{
		if (effects.TryGetValue(type, out GameObject prefab))
		{
			GameObject effectObj = Instantiate(prefab, position, rotation);
			return effectObj.GetComponent<TEffect>();
		}

		return null;
	}

	public TEffect Instantiate<TEffect>(EffectType type, Transform parent, Vector3 position, Quaternion rotation)
								where TEffect : Component
	{
		if (effects.TryGetValue(type, out GameObject prefab))
		{
			GameObject effectObj = Instantiate(prefab, position, rotation);
			effectObj.transform.SetParent(parent, true);
			return effectObj.GetComponent<TEffect>();
		}

		return null;
	}

	public TEffect Instantiate<TEffect>(EffectType type, RaycastHit2D hitInfo)
								where TEffect : Component
	{
		if (effects.TryGetValue(type, out GameObject prefab))
		{
			GameObject effectObj = Instantiate(prefab);

			effectObj.transform.position = hitInfo.point;
			effectObj.transform.rotation = Quaternion.LookRotation(hitInfo.normal, Vector3.right);

			return effectObj.GetComponent<TEffect>();
		}

		return null;
	}

	public TEffect Instantiate<TEffect>(EffectType type, Vector3 position, Vector3 normal)
								where TEffect : Component
	{
		if (effects.TryGetValue(type, out GameObject prefab))
		{
			GameObject effectObj = Instantiate(prefab);

			effectObj.transform.position = position;
			effectObj.transform.rotation = Quaternion.LookRotation(normal, Vector3.right);

			return effectObj.GetComponent<TEffect>();
		}

		return null;
	}
	#endregion
}


public enum EffectType
{
	SolidImpact,
	CreatureImpact,
	CreatureDeath,
	MuzzleFlash,
	BulletTracer,
	BloodDripping,
}