using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class Inventory : ItemContainer, IPointerEnterHandler, IPointerExitHandler
{
	public static Inventory Instance { get; private set; }

	[Header("References"), Space]
	[ReadOnly] public bool insideInventory;
	[SerializeField] private CanvasGroup canvasGroup;

	[Header("Items List"), Space]
	public List<Item> preplacedItems = new List<Item>();
	public List<Item> items = new List<Item>();

	// Private fields.
	private InventorySlot[] _slots;
	private bool _initialized;

	protected override void Awake()
	{
		if (Instance == null)
			Instance = this;
		else
		{
			Debug.LogWarning("More than one Instance of Inventory found!!");
			Destroy(gameObject);
			return;
		}

		_slots = transform.GetComponentsInChildren<InventorySlot>("Slots");

		base.Awake();
	}

	private IEnumerator Start()
	{
		if (!_initialized)
		{
			Debug.Log("Inventory initializing...");
			foreach (Item itemSO in preplacedItems)
			{
				Item currentItem = Instantiate(itemSO);
				currentItem.name = itemSO.name;
				Add(currentItem);
			}

			preplacedItems.Clear();

			if (HasAny("Pistol", out Item pistol))
				PlayerActions.weapons[0] = pistol as Weapon;

			if (HasAny("Knife", out Item knife))
				PlayerActions.weapons[1] = knife as Weapon;

			if (HasAny("Flashlight", out Item flashlight))
			{
				Electronic flashLight = flashlight as Electronic;
				flashLight.IsTurnedOn = true;
				PlayerActions.flashlight = flashLight;
			}

			yield return new WaitForSecondsRealtime(.1f);
			PlayerActions.Instance.SwitchWeapon(0);

			_initialized = true;
		}

		InputManager.Instance.onToggleInventoryAction += InputManager_OnToggleInventoryAction;
		Toggle(false);
	}

	#region Event Methods.
	public void OnPointerEnter(PointerEventData eventData)
	{
		insideInventory = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		insideInventory = false;
	}

	private void InputManager_OnToggleInventoryAction(object sender, EventArgs e)
	{
		if (!PlayerStats.IsDeath)
			Toggle(!canvasGroup.interactable);
	}

	public void Toggle(bool state)
	{
		Debug.Log("Inventory disabled.");
		canvasGroup.Toggle(state);

		if (!state)
		{
			ClickableObject.CleanUpStatics();
			insideInventory = false;
		}
	}
	#endregion

	#region Inherited Methods.
	public override bool Add(Item target, bool forcedSplit = false)
	{
		bool success = base.AddToList(items, target, forcedSplit, out bool outOfSpace);

		if (outOfSpace)
			Toggle(true);

		return success;
	}

	public override void Remove(Item target, bool forced = false)
	{
		if (!target.isFavorite || forced)
		{
			items.Remove(target);
			onItemChanged?.Invoke();
		}
	}

	public override void Remove(string targetID, bool forced = false)
	{
		Item target = GetItem(targetID);

		if (!target.isFavorite || forced)
		{
			items.Remove(target);
			onItemChanged?.Invoke();
		}
	}

	public override void RemoveWithoutNotify(Item target)
	{
		items.Remove(target);
	}

	public override Item GetItem(string targetID)
	{
		return items.Find(item => item.id.Equals(targetID));
	}

	public override Item[] GetItemsByName(string targetName)
	{
		return items.FindAll(item => item.itemName.Equals(targetName)).ToArray();
	}

	public override bool HasAny(string targetName)
	{
		return items.Find(item => item.itemName.Equals(targetName)) != null;
	}

	public override bool HasAny(string targetName, out Item firstFound)
	{
		firstFound = items.Find(item => item.itemName.Equals(targetName));

		return firstFound != null;
	}

	public override bool IsExisting(string targetID)
	{
		return items.Exists(item => item.id == targetID);
	}

	public override void UpdateQuantity(string targetID, int amount, bool setExactAmount = false)
	{
		Item target = GetItem(targetID);

		base.SetQuantity(target, amount, setExactAmount);

		if (target.quantity <= 0)
		{
			Remove(target, true);
			return;
		}

		onItemChanged?.Invoke();
	}

	public void UpdateItemTooltip(int slotIndex)
	{
		_slots[slotIndex].UpdateTooltipContent();
	}

	protected override void ReloadUI()
	{
		// Split the master list into 2 smaller lists.
		List<Item> unindexedItems = items.FindAll(item => item.slotIndex == -1);
		List<Item> indexedItems = items.FindAll(item => item.slotIndex != -1);

		// Clear all the _slots.
		Array.ForEach(_slots, (slot) => slot.ClearItem());

		// Load the indexed items first.
		if (indexedItems.Count != 0)
		{
			Action<Item> ReloadIndexedItems = (item) => _slots[item.slotIndex].AddItem(item);
			indexedItems.ForEach(ReloadIndexedItems);
		}

		// Secondly, load the unindexed items to the leftover empty slots.
		if (unindexedItems.Count != 0)
		{
			int i = 0;

			foreach (InventorySlot slot in _slots)
			{
				if (i == unindexedItems.Count)
					break;

				if (slot.currentItem == null)
				{
					unindexedItems[i].slotIndex = slot.transform.GetSiblingIndex();

					slot.AddItem(unindexedItems[i]);

					i++;
				}
			}
		}

		// Update the master list.
		items.Clear();
		items.AddRange(unindexedItems);
		items.AddRange(indexedItems);

		// Sort the list by slot indexes in ascending order.
		items.Sort((a, b) => a.slotIndex.CompareTo(b.slotIndex));
	}
	#endregion
}
