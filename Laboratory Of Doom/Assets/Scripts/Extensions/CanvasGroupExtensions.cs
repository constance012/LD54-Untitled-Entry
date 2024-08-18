using UnityEngine;

public static class CanvasGroupExtensions
{
	public static void Toggle(this CanvasGroup canvasGroup, bool state)
	{
		canvasGroup.alpha = state ? 1f : 0f;
		canvasGroup.interactable = state;
		canvasGroup.blocksRaycasts = state;
	}
}