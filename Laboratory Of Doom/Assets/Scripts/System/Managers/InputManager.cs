using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityDebug = UnityEngine.Debug;

/// <summary>
/// Manages inputs from various devices and sources, using the NEW input system.
/// </summary>
public class InputManager : Singleton<InputManager>
{
	public EventHandler onToggleInventoryAction;
	public EventHandler onBackToMenuAction;

	private PlayerInputActions _playerInputActions;
	private Dictionary<KeybindingActions, InputAction> _inputActions;

	protected override void Awake()
	{
		base.Awake();

		Initialize();
	}

	private void OnDestroy()
	{
		Dispose();
	}

	#region Event Methods.
	private void ToggleInventory_performed(InputAction.CallbackContext context)
	{
		onToggleInventoryAction?.Invoke(this, EventArgs.Empty);
	}

	private void BackToMenu_performed(InputAction.CallbackContext context)
	{
		onBackToMenuAction?.Invoke(this, EventArgs.Empty);
	}
	#endregion

	#region Public Wrapper Method.
	public Vector2 Read2DVector(KeybindingActions action)
	{
		return _inputActions[action].ReadValue<Vector2>().normalized;
	}

	public string GetDisplayString(KeybindingActions action, int index = 0)
	{
		ReadOnlyArray<InputBinding> bindings = _inputActions[action].bindings;
		index = Mathf.Clamp(index, 0, bindings.Count - 1);

		return bindings[index].ToDisplayString();
	}

	public bool WasPressedThisFrame(KeybindingActions action)
	{
		return _inputActions[action].WasPressedThisFrame();
	}

	public bool WasPerformedThisFrame(KeybindingActions action)
	{
		return _inputActions[action].WasPerformedThisFrame();
	}

	public bool WasReleasedThisFrame(KeybindingActions action)
	{
		return _inputActions[action].WasReleasedThisFrame();
	}
	#endregion

	private void Initialize()
	{
		_playerInputActions = new PlayerInputActions();

		_playerInputActions.Player.Enable();

		_playerInputActions.Player.ToggleInventory.performed += ToggleInventory_performed;
		_playerInputActions.Player.BackToMenu.performed += BackToMenu_performed;

		_inputActions ??= new Dictionary<KeybindingActions, InputAction>()
		{
			[KeybindingActions.Attack] = _playerInputActions.Player.Attack,

			[KeybindingActions.MoveLeft] = _playerInputActions.Player.Movement,
			[KeybindingActions.MoveRight] = _playerInputActions.Player.Movement,
			[KeybindingActions.MoveUp] = _playerInputActions.Player.Movement,
			[KeybindingActions.MoveDown] = _playerInputActions.Player.Movement,

			[KeybindingActions.PrimaryWeapon] = _playerInputActions.Player.PrimaryWeapon,
			[KeybindingActions.SecondaryWeapon] = _playerInputActions.Player.SecondaryWeapon,

			[KeybindingActions.Inventory] = _playerInputActions.Player.ToggleInventory,
			[KeybindingActions.Flashlight] = _playerInputActions.Player.Flashlight,
			[KeybindingActions.Interact] = _playerInputActions.Player.Interact,
			[KeybindingActions.Reload] = _playerInputActions.Player.Reload,
			
			[KeybindingActions.BackToMenu] = _playerInputActions.Player.BackToMenu,
		};
	}

	private void Dispose()
	{
		_playerInputActions.Player.ToggleInventory.performed -= ToggleInventory_performed;
		_playerInputActions.Player.BackToMenu.performed -= BackToMenu_performed;
		
		_playerInputActions.Dispose();
	}
}

/// <summary>
/// Represents different actions in the game associated with different control keys.
/// </summary>
public enum KeybindingActions
{
	None,
	Attack,
	MoveLeft,
	MoveRight,
	MoveUp,
	MoveDown,
	PrimaryWeapon,
	SecondaryWeapon,
	Inventory,
	Interact,
	BackToMenu,
	Flashlight,
	Reload
}