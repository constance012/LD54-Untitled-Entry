using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Linq;

public class LevelsNavigationDoor : Interactable
{
	public enum DoorDirection { ToNextLevel, ToPreviousLevel, ToMainMenu, RemainCurrentLevel }
	
	[Header("UI"), Space]
	[SerializeField] private TextMeshProUGUI _levelNameText;

	[Header("Status"), Space]
	[ReadOnly] public bool isOpened;

	[Header("Door Settings"), Space]
	public Item[] keycards;
	public DoorDirection direction;
	public string doorDescription;

	[Header("Sprites"), Space]
	[SerializeField] private Sprite openSprite;
	[SerializeField] private Sprite closeSprite;

	// Private fields.
	private Transform _enemiesContainer;
	private BoxCollider2D _collider;
	private bool _levelCleared;

	protected override void Awake()
	{
		base.Awake();
		
		_enemiesContainer = GameObject.FindWithTag("EnemyContainer").transform;
		_collider = GetComponent<BoxCollider2D>();
	}

	private IEnumerator Start()
	{
		if (direction == DoorDirection.ToNextLevel)
		{
			yield return new WaitForSeconds(1f);

			_levelNameText = GameObject.FindWithTag("LevelNameText").GetComponent<TextMeshProUGUI>();
			_levelNameText.text = LevelsManager.Instance.CurrentScene.name.ToUpper();

			DisplayLevelText();
		}

		_spriteRenderer.sprite = closeSprite;
		isOpened = false;
	}

	private void LateUpdate()
	{
		if (_levelCleared || direction != DoorDirection.ToNextLevel)
			return;

		bool isCleared = _enemiesContainer.childCount == 0;

		if (_levelCleared != isCleared)
		{
			_levelCleared = true;
			_levelNameText.text = "<color=#C88529> THANK YOU! </color> FOR RELEASING OUR SOULS";

			DisplayLevelText();
		}
	}

	protected override void TriggerInteraction(float playerDistance)
	{
		base.TriggerInteraction(playerDistance);

		if (InputManager.Instance.WasPressedThisFrame(KeybindingActions.Interact) && !hasInteracted)
			Interact();
	}

	public override void Interact()
	{
		base.Interact();

		if (CheckForKeycards())
		{
			isOpened = true;
			hasInteracted = true;

			_spriteRenderer.sprite = openSprite;
			gameObject.layer = LayerMask.NameToLayer("Door");

			PathRequester.Instance.ChangeGridCellState(transform.position, true);
			
			Enter();
		}
	}

	protected override void CreatePopupLabel()
	{
		if (!isOpened)
		{
			base.CreatePopupLabel();

			string text = "REQUIRES TO UNLOCK:\n";

			if (keycards.Length == 0)
				text = "REQUIRES TO UNLOCK: NONE.";
			else
				foreach (Item keycard in keycards)
					text += $"<color=#{ColorUtility.ToHtmlStringRGB(keycard.rarity.color)}> {keycard.itemName} </color>,\n";

			_clone.SetObjectName(text);
		}
	}

	private bool CheckForKeycards()
	{
		bool allCardMatched = keycards.All(keycard => Inventory.Instance.HasAny(keycard.itemName));
		return allCardMatched;
	}

	private void Enter()
	{
		if (isOpened)
		{
			switch (direction)
			{
				case DoorDirection.ToNextLevel:
					GameManager.Instance.ShowVictoryScreen();
					break;

				case DoorDirection.ToPreviousLevel:
					LevelsManager.Instance.LoadPreviousLevel();
					break;

				case DoorDirection.ToMainMenu:
					GameManager.Instance.ReturnToMenu();
					break;

				case DoorDirection.RemainCurrentLevel:
					_collider.enabled = false;
					break;
			}

			hasInteracted = true;
		}
	}

	private void DisplayLevelText()
	{
		CanvasGroup canvasGroup = _levelNameText.GetComponent<CanvasGroup>();

		canvasGroup.alpha = 0f;
		
		Sequence sequence = DOTween.Sequence();

		sequence.Append(canvasGroup.DOFade(1f, .75f))
				.AppendInterval(2f)
				.Append(canvasGroup.DOFade(0f, .75f));
	}
}
