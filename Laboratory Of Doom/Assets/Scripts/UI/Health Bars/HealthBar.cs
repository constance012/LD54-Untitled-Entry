using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class HealthBar : MonoBehaviour
{
	[Header("UI References"), Space]
	[SerializeField] private Slider mainSlider;
	[SerializeField] private Slider fxSlider;
	[Space, SerializeField] private TextMeshProUGUI displayText;

	[Header("Colors and Gradients"), Space]
	[SerializeField] private Gradient healthGradient;
	[Space, SerializeField] private Color healthIncreaseColor;
	[SerializeField] private Color healthDecreaseColor;

	[Header("Effect Settings"), Space]
	[SerializeField] private float fxDelay;
	[SerializeField] private float fxDuration;

	public bool IsPreviousEffectActive => _fxTween.IsActive();

	// Private fields.
	private Image _mainFillRect;
	private Image _fxFillRect;
	private Tween _fxTween;

	protected virtual void Awake()
	{
		_mainFillRect = mainSlider.fillRect.GetComponent<Image>();
		_fxFillRect = fxSlider.fillRect.GetComponent<Image>();
	}

	public void SetCurrentHealth(int current)
	{
		// Health decreasing.
		if (current <= mainSlider.value)
		{
			_fxFillRect.color = healthDecreaseColor;

			mainSlider.value = current;
			_mainFillRect.color = healthGradient.Evaluate(mainSlider.normalizedValue);
		}

		// Health increasing.
		else
		{
			_fxFillRect.color = healthIncreaseColor;
			fxSlider.value = current;
		}

		displayText.text = $"{current} / {mainSlider.maxValue}";

		if (IsPreviousEffectActive)
			_fxTween.Kill(true);

		_fxTween = PerformEffect();
	}

	public void SetMaxHealth(int max, bool initialize = true)
	{
		mainSlider.maxValue = max;
		fxSlider.maxValue = max;
		
		if (initialize)
		{
			mainSlider.value = max;
			fxSlider.value = max;

			_mainFillRect.color = healthGradient.Evaluate(mainSlider.normalizedValue);
			displayText.text = $"{max} / {max}";
		}
	}

	private Tween PerformEffect()
	{
		if (_fxFillRect.color == healthIncreaseColor)
		{
			Sequence sequence = DOTween.Sequence();

			sequence.Append(mainSlider.DOValue(fxSlider.value, fxDuration).SetEase(Ease.OutCubic))
					.Join(_mainFillRect.DOColor(healthGradient.Evaluate(fxSlider.normalizedValue), fxDuration))
					.SetDelay(fxDelay);

			return sequence;
		}
		else
		{
			return fxSlider.DOValue(mainSlider.value, fxDuration).SetEase(Ease.OutCubic).SetDelay(fxDelay);
		}
	}
}
