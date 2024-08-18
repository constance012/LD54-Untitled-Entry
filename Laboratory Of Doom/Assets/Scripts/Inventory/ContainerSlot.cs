using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class ContainerSlot : MonoBehaviour
{
	[Header("Current Item"), Space]
	public Item currentItem;

	// Protected fields.
	protected Image icon;
	protected TextMeshProUGUI quantity;

	protected TooltipTrigger tooltip;

	protected virtual void Awake()
	{
		icon = transform.GetComponentInChildren<Image>("Item Button/Icon");

		quantity = transform.GetComponentInChildren<TextMeshProUGUI>("Item Button/Quantity");
		tooltip = GetComponent<TooltipTrigger>();
	}

	private void Start()
	{
		icon.gameObject.SetActive(false);
	}

	public virtual void AddItem(Item newItem)
	{
		currentItem = newItem;

		icon.sprite = currentItem.icon;
		icon.gameObject.SetActive(true);
		
		quantity.text = currentItem.stackable ? currentItem.quantity.ToString() : "";

		tooltip.header = currentItem.itemName;
		tooltip.content = currentItem.ToString();
		tooltip.popupDelay = .4f;
	}

	public virtual void ClearItem()
	{
		currentItem = null;

		icon.sprite = null;
		icon.gameObject.SetActive(false);

		quantity.text = "";

		tooltip.header = "";
		tooltip.content = "";
		tooltip.popupDelay = 0f;
	}

	public void UseItem()
	{
		// Use the item if it's not null and be used.
		if (currentItem != null && currentItem.canBeUsed)
			currentItem.Use();
	}

	public void UpdateTooltipContent()
	{
		tooltip.content = currentItem.ToString();
	}

	public abstract void OnDrop(GameObject shipper);

	protected void SwapSlotIndexes<TSlot>(ClickableObject cloneData) where TSlot : ContainerSlot
	{
		Item droppedItem = cloneData.dragItem;
		ItemContainer currentStorage = cloneData.currentStorage;

		int senderIndex = droppedItem.slotIndex;
		int destinationIndex = currentItem.slotIndex;

		// If swap within the current storage.
		if (cloneData.FromSameStorageSlot<TSlot>())
		{
			if (!currentStorage.IsExisting(droppedItem.id))
				currentStorage.Add(droppedItem, true);

			// Swap their slot indexes.
			currentStorage.UpdateSlotIndex(currentItem.id, senderIndex);
			currentStorage.UpdateSlotIndex(droppedItem.id, destinationIndex);
		}
	}
}
