using System;
using UnityEngine;
using DG.Tweening;

[Serializable]
public class UITweener
{
	[Header("Tweener Settings"), Space]
	[Header("General"), Space]
	public string name;
	public UITweeningType tweenType;
	
	[Tooltip("The end value of the tween, USE ONLY the X component for a single floating point.")]
	public Vector3 endValue;
	public float duration;

	[Header("On Game Starts"), Space]
	[Tooltip("The override value, USE ONLY the X component for a single floating point.")]
	public Vector3 startValue;
	public bool useCurrentValueAsStart;
	
	[Tooltip("Override the starting value of the game object after the game starts.")]
	public bool overrideStartValue;
	
	[Header("Moving and Rotating"), Space]
	public RotateMode rotateMode;
	public bool snapToInteger;
	public bool isSpeedBased;

	[Header("Looping"), Space]
	public LoopType loopType;
	public int loopCount;

	[Header("Easing and Delay"), Space]
	public Ease easeType = Ease.Linear;
	public float delay;

	[Header("Back, Elastic and Flash Eases Only"), Space]
	public float overshoot = 1.70158f;
	[Range(-1f, 1f)] public float period = 0f;

	[Header("Custom Update Method"), Space]
	public UpdateType updateType;
	public bool ignoreTimeScale;

	[Header("Others"), Space]
	public bool tweenInRelativeSpace;
	public bool playAlongPreviousTweener;

	public TweenableUIElement TweenableUI { get; set; }

	private Tween _tween;

	public void ValidateDefaultValues(int index)
	{
		name = $"Unnamed Tweener #{index + 1}";
		easeType = Ease.Linear;
		overshoot = 1.70158f;
	}

	public void KillIfActive()
	{
		if (_tween.IsActive())
			_tween.Kill(true);
	}

	public Tween CreateTween(bool forwards, bool standalone = true)
	{
		KillIfActive();

		Vector3 targetValue = forwards ? endValue : startValue;

		switch (tweenType)
		{
			case UITweeningType.Scale:
				_tween = TweenableUI._rectTransform.DOScale(targetValue, duration);
				break;

			case UITweeningType.SizeDelta:
				_tween = TweenableUI._rectTransform.DOSizeDelta(targetValue, duration, snapToInteger);
				break;

			case UITweeningType.Move:
				_tween = TweenableUI._rectTransform.DOAnchorPos(targetValue, duration, snapToInteger);
				break;

			case UITweeningType.Rotate:
				_tween = TweenableUI._rectTransform.DORotate(targetValue, duration, rotateMode);
				break;

			case UITweeningType.FadeCanvasGroup:
				_tween = TweenableUI._canvasGroup.DOFade(targetValue.x, duration);
				break;
			
			case UITweeningType.FadeGraphic:
				_tween = TweenableUI._graphic.DOFade(targetValue.x, duration);
				break;

			case UITweeningType.Color:
				_tween = TweenableUI._graphic.DOColor(TweenableUI.Vector3ToColor(targetValue), duration);
				break;
		}

		_tween.SetRelative(tweenInRelativeSpace)
			 .SetLoops(loopCount, loopType)
			 .SetEase(easeType, overshoot, period)
			 .SetSpeedBased(isSpeedBased)
			 .SetUpdate(updateType, ignoreTimeScale);

		if (standalone)
			_tween.SetDelay(delay);

		return _tween;
	}
}

public enum UITweeningType
{
	Scale = 0,
	SizeDelta = 6,
	Move = 1,
	Rotate = 2,
	FadeCanvasGroup = 3,
	FadeGraphic = 4,
	Color = 5
}

public enum TweenCallbackPeriod
{
	AfterForwardTween,
	AfterBackwardTween
}