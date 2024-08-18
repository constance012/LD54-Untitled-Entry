using UnityEngine;

public class TooltipHandler : Singleton<TooltipHandler>
{
	[SerializeField] private Tooltip tooltip;
	
	// Private fields.
	private bool _isShowed;

	public static void Show(string contentText, string headerText = "")
	{
		if (!Instance._isShowed)
		{
			Instance.tooltip.SetText(contentText, headerText);

			Instance.tooltip.gameObject.SetActive(true);
			Instance._isShowed = true;
		}
	}

	public static void Hide()
	{
		if (Instance._isShowed)
		{
			Instance.tooltip.gameObject.SetActive(false);
			Instance._isShowed = false;
		}
	}
}
