using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	[Header("Settings"), Space]
	public string header;
	[TextArea(5, 10)] public string content;
	public float popupDelay;

	// Private static fields.
	private static Tween _showTween;

	public void OnPointerEnter(PointerEventData eventData)
	{
		ShowTooltip();
	}
	
	public void OnPointerExit(PointerEventData eventData)
	{
		HideTooltip();
	}

	public void OnMouseEnter()
	{
		ShowTooltip();
	}

	public void OnMouseExit()
	{
		HideTooltip();
	}

	public void HideTooltip()
	{
		TooltipHandler.Hide();
		
		if (_showTween.IsActive())
			_showTween.Kill();
	}

	private void ShowTooltip()
	{
		if (!string.IsNullOrEmpty(header) || !string.IsNullOrEmpty(content))
		{
			_showTween = DOVirtual.DelayedCall(popupDelay, () => TooltipHandler.Show(content, header));
		}
	}
}