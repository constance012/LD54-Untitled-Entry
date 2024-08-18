using UnityEngine;

/// <summary>
/// Base class for all interactable objects.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public abstract class Interactable : MonoBehaviour
{
	public enum InputSource { Mouse, Keyboard, Joystick, None }

	public enum InteractableType
	{
		/// <summary>
		/// Can only be controlled by other mechanisms.
		/// </summary>
		Passive,

		/// <summary>
		/// Can either be controlled by other mechanisms or interacted by the player.
		/// </summary>
		Active,

		/// <summary>
		/// Can only be interacted manually by the player.
		/// </summary>
		Manual
	}

	[Header("Type"), Space]
	public InteractableType type;
	public InputSource inputSource;

	[Header("Reference"), Space]
	public Transform player;
	[SerializeField] protected GameObject popupLabelPrefab;

	[Header("Interaction Radius"), Space]
	[SerializeField, Tooltip("The distance required for the player to interact with this object.")]
	protected float interactDistance;
	
	[SerializeField, ReadOnly] protected bool hasInteracted;

	// Protected fields.
	protected Transform _worldCanvas;
	protected SpriteRenderer _spriteRenderer;
	protected Material _mat;
	protected InteractionPopupLabel _clone;

	protected virtual void Awake()
	{
		if (player == null)
			player = GameObject.FindWithTag("Player").transform;

		_worldCanvas = GameObject.FindWithTag("WorldCanvas").transform;
		_spriteRenderer = GetComponent<SpriteRenderer>();
		_mat = _spriteRenderer.material;
	}

	protected void Update()
	{
		if (type == InteractableType.Passive)
			return;

		Vector2 worldMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

		float mouseDistance = Vector2.Distance(worldMousePos, transform.position);
		float playerDistance = Vector2.Distance(player.position, transform.position);

		CheckForInteraction(mouseDistance, playerDistance);
	}

	public virtual void Interact()
	{
		Debug.Log($"Interacting with {transform.name}.");
	}

	protected virtual void CheckForInteraction(float mouseDistance, float playerDistance)
	{
		if (playerDistance <= interactDistance)
		{
			TriggerInteraction(playerDistance);
		}

		else
		{
			CancelInteraction(playerDistance);
		}
	}

	protected virtual void TriggerInteraction(float playerDistance)
	{
		if (_clone == null)
			CreatePopupLabel();
		else
			_clone.transform.position = transform.position;

		_mat.SetFloat("_Thickness", 1f);

		// TODO - derived classes implement their own way to trigger interaction.
	}

	protected virtual void CancelInteraction(float playerDistance)
	{
		if (_clone != null)
			Destroy(_clone.gameObject);

		_mat.SetFloat("_Thickness", 0f);

		// TODO - derived classes implement their own way to cancel interaction.
	}

	protected virtual void CreatePopupLabel()
	{
		GameObject label = Instantiate(popupLabelPrefab);
		label.name = popupLabelPrefab.name;

		_clone = label.GetComponent<InteractionPopupLabel>();

		_clone.SetupLabel(transform, inputSource);
	}

	protected virtual void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, interactDistance);
	}
}
