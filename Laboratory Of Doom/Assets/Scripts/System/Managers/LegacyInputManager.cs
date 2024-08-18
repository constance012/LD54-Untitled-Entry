using UnityEngine;
using static Keyset;

/// <summary>
/// Manages all the keyboard input for the game, using the OLD input system.
/// </summary>
public class LegacyInputManager : Singleton<LegacyInputManager>
{
	[Header("Keyset Reference")]
	[Space]
	[SerializeField] private Keyset keySet;

	public KeyCode GetKeyForAction(KeybindingActions action)
	{
		foreach (Key key in keySet.keys.list)
			if (key.action == action)
				return key.keyCode;

		return KeyCode.None;
	}

	/// <summary>
	/// Returns true while the user holds down the key for the specified action.
	/// </summary>
	/// <param name="action"></param>
	/// <returns></returns>
	public bool GetKey(KeybindingActions action)
	{
		KeyCode keyCode = GetKeyForAction(action);
		bool result = Input.GetKey(keyCode);

		return result;
	}

	/// <summary>
	/// Returns true during the frame the user starts pressing down the key for the specified action.
	/// </summary>
	/// <param name="action"></param>
	/// <returns></returns>
	public bool GetKeyDown(KeybindingActions action)
	{
		KeyCode keyCode = GetKeyForAction(action);
		//Debug.Log(keyCode);
		bool result = Input.GetKeyDown(keyCode);

		return result;
	}

	/// <summary>
	/// Returns true during the frame the user releases the key for the specified action.
	/// </summary>
	/// <param name="action"></param>
	/// <returns></returns>
	public bool GetKeyUp(KeybindingActions action)
	{
		KeyCode keyCode = GetKeyForAction(action);
		bool result = Input.GetKeyUp(keyCode);

		return result;
	}

	/// <summary>
	/// Returns the value of the axis based on which key is being held.
	/// </summary>
	/// <param name="axis"></param>
	/// <returns></returns>
	public float GetAxisRaw(string axis)
	{
		axis = axis.ToLower().Trim();
		
		switch (axis)
		{
			case "horizontal":
				if (GetKey(KeybindingActions.MoveRight))
					return 1f;
				
				else if (GetKey(KeybindingActions.MoveLeft))
					return -1f;

				else
					return 0f;

			case "vertical":
				if (GetKey(KeybindingActions.MoveUp))
					return 1f;
				
				else if (GetKey(KeybindingActions.MoveDown))
					return -1f;

				else
					return 0f;
		}

		return 0f;
	}
}
