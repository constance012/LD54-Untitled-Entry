using TMPro;
using UnityEngine;
using static Interactable;

public class InteractionPopupLabel : MonoBehaviour
{
	[Header("References")]
	[Space]
	[SerializeField] private Animator animator;

	[Space]
	[SerializeField] private TextMeshProUGUI label;
	[SerializeField] private TextMeshProUGUI keyboardCue;
	[SerializeField] private Transform mouseCue;

	[Space]
	[SerializeField] private Transform worldCanvas;

	private void Awake()
	{
		worldCanvas = GameObject.FindWithTag("WorldCanvas").transform;
	}

	public void RestartAnimation()
	{
		animator.SetTrigger("Restart");
	}

	public void SetObjectName(string name)
	{
		label.text = name.Trim().ToUpper();
	}

	public void SetObjectName(string name, int quantity, Color textColor, bool isDuplicated = false)
	{
		TextMeshProUGUI chosenLabel;

		if (isDuplicated)
			chosenLabel = Instantiate(label, label.transform.parent);
		else
			chosenLabel = label;

		chosenLabel.text = quantity > 1 ? $"{name.ToUpper()} x{quantity}" : name.ToUpper();
		chosenLabel.color = textColor;
	}

	public void SetupLabel(Transform interactable, InputSource inputSource)
	{
		SetObjectName("");

		keyboardCue.text = InputManager.Instance.GetDisplayString(KeybindingActions.Interact).ToUpper();

		switch (inputSource)
		{
			case InputSource.Mouse:
				keyboardCue.gameObject.SetActive(false);
				mouseCue.gameObject.SetActive(true);
				break;

			case InputSource.Keyboard:
				keyboardCue.gameObject.SetActive(true);
				mouseCue.gameObject.SetActive(false);
				break;

			case InputSource.Joystick:
				break;

			case InputSource.None:
				mouseCue.parent.gameObject.SetActive(false);
				break;
		}

		transform.position = interactable.position;
		transform.SetParent(worldCanvas, true);
		transform.SetAsLastSibling();
	}
}
