using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ClickableObject : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
{
	public enum StorageType { Inventory, Chest }

	[Header("Storage Type"), Space]
	public StorageType storageType;
	[ReadOnly] public ItemContainer currentStorage;

	[Header("References"), Space]
	public Item dragItem;
	[SerializeField] private GameObject droppedItemPrefab;
	[SerializeField] private Transform player;

	public bool IsChestSlot => storageType == StorageType.Chest;
	public bool IsInventorySlot => storageType == StorageType.Inventory;
	public bool FromSameStorageSlot<TSlot>() where TSlot : ContainerSlot => _currentSlot.GetType() == typeof(TSlot);

	// Private fields.
	private Transform _parentCanvas;
	private ContainerSlot _currentSlot;
	private Image _icon;
	private TooltipTrigger _tooltip;

	private bool _isLeftAltHeld;
	private bool _isLeftControlHeld;

	private bool _isCoroutineRunning;

	// Static properties.

	// A clone that "ships" the current item in this slot.
	public static GameObject Clone { get; set; }
	// Singleton reference to the sender script.
	public static ClickableObject Sender { get; private set; }
	public static bool HoldingItem { get; set; }
	
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void ResetStatic()
	{
		CleanUpStatics();
	}

	private void Awake()
	{
		currentStorage = GetComponentInParent<Inventory>();

		_parentCanvas = GetComponentInParent<Canvas>().transform;
		_currentSlot = GetComponentInParent<ContainerSlot>();
		_icon = transform.GetComponentInChildren<Image>("Icon");
		_tooltip = GetComponentInParent<TooltipTrigger>();
		
		player = GameObject.FindWithTag("Player").transform;
	}

	private void Update()
	{
		if (_isCoroutineRunning)
			Debug.Log("Coroutine is running.", this);

		_isLeftAltHeld = Input.GetKey(KeyCode.LeftAlt);
		_isLeftControlHeld = Input.GetKey(KeyCode.LeftControl);

		if (Clone != null)
			Clone.transform.position = Input.mousePosition;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		dragItem = _currentSlot.currentItem;
		_tooltip.HideTooltip();

		if (dragItem == null && !HoldingItem)
			return;

		if (eventData.button == PointerEventData.InputButton.Left)
		{			
			// Pick item up.
			if (!HoldingItem)
			{
				BeginDragItem();
				return;
			}

			// Drop item down.
			if (HoldingItem)
			{
				_currentSlot.OnDrop(Clone);

				CleanUpStatics();
				return;
			}
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Right && !_isLeftAltHeld && !_isLeftControlHeld)
		{
			_currentSlot?.UseItem();
			CleanUpStatics();
		}

		if (eventData.button == PointerEventData.InputButton.Left && _isLeftControlHeld)
			DisposeItem();
	}

	#region Item Manipulation.
	public static void UseCurrentlyHoldingItem()
	{
		Sender._currentSlot.UseItem();
		CleanUpStatics();
	}

	public void DisposeItem()
	{
		if (Clone == null)
			return;

		Debug.Log("Disposing item...");

		Item disposeItem = Clone.GetComponent<ClickableObject>().dragItem;

		if (!disposeItem.isFavorite || Sender.IsChestSlot)
		{
			// Set up the drop.
			ItemPickup droppedItem = droppedItemPrefab.GetComponent<ItemPickup>();

			droppedItem.itemSO = disposeItem;
			droppedItem.itemSO.slotIndex = -1;
			droppedItem.itemSO.isFavorite = false;
			droppedItem.player = player;

			// Make the drop.
			GameObject droppedItemObj = Instantiate(droppedItemPrefab, player.position, Quaternion.identity);

			droppedItemObj.name = disposeItem.name;
			droppedItemObj.transform.SetParent(GameObject.FindWithTag("ItemHolder").transform);

			// Add force to the dropped item.
			Rigidbody2D rb2d = droppedItemObj.GetComponent<Rigidbody2D>();

			Vector3 screenPlayerPos = Camera.main.WorldToScreenPoint(player.position);
			Vector3 aimingDir = Input.mousePosition - screenPlayerPos;

			rb2d.AddForce(5f * aimingDir.normalized, ForceMode2D.Impulse);

			currentStorage.Remove(disposeItem);
			
			if (disposeItem.category == ItemCategory.Weapon)
			{
				Weapon weapon = disposeItem as Weapon;
				int slotIndex = (int)weapon.weaponSlot;

				PlayerActions.weapons[slotIndex] = null;
				PlayerActions.Instance.SwitchWeapon(slotIndex);
			}

			if (disposeItem.itemName.Equals("Flashlight") && disposeItem.category == ItemCategory.Electronic)
				PlayerActions.flashlight = null;
		}

		CleanUpStatics();
	}

	public static void CleanUpStatics()
	{
		if (Sender != null && Clone != null)
		{
			Sender._icon.color = Color.white;

			Sender.dragItem = null;

			Sender = null;
			HoldingItem = false;

			Destroy(Clone);
		}

		if (Inventory.Instance != null)
			Inventory.Instance.onItemChanged?.Invoke();
	}

	private void BeginDragItem()
	{
		CreateClone();

		_icon.color = new Color(.51f, .51f, .51f);

		HoldingItem = true;
		Sender = this;

		Debug.Log("You're dragging the " + dragItem.itemName);
	}

	private void CreateClone()
	{
		Clone = Instantiate(gameObject, _parentCanvas);

		Clone.GetComponent<RectTransform>().pivot = new Vector2(.48f, .55f);

		Clone.GetComponent<Image>().enabled = false;
		Clone.transform.GetComponentInChildren<Image>("Icon").raycastTarget = false;

		ClickableObject cloneData = Clone.GetComponent<ClickableObject>();
		cloneData.currentStorage = currentStorage;
		cloneData._currentSlot = _currentSlot;
	}
	#endregion
}
