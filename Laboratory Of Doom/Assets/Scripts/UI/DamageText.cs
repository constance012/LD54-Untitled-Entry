using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public enum DamageTextStyle { Small, Normal, Critical }

/// <summary>
/// A class to generates an UI popup text.
/// </summary
public class DamageText : MonoBehaviour
{
	[Header("References"), Space]
	[SerializeField] private TextMeshProUGUI displayText;
	[SerializeField] private CanvasGroup canvasGroup;
	
	[Header("Configurations"), Space]
	[Min(1f)] public float maxLifeTime = 1f;
	[Space, Min(.5f)] public float maxVelocity;

	// Color Constants.
	public static readonly Color CriticalColor = new Color(.821f, .546f, .159f);
	public static readonly Color DamageColor = Color.red;
	public static readonly Color HealingColor = Color.green;
	public static readonly Color PowerUpColor = new Color(.707f, .11f, .353f);
	public static readonly Color BonusScoreColor = new Color(.792f, .13f, .714f);

	// Style scales.
	private static readonly Dictionary<DamageTextStyle, float> _styleScales = new Dictionary<DamageTextStyle, float>()
	{
		[DamageTextStyle.Small] = .15f,
		[DamageTextStyle.Normal] = .2f,
		[DamageTextStyle.Critical] = .25f
	};

	// Private fields.
	private Color _currentTextColor;
	private Sequence _idleSequence;
	private bool _criticalHit;

	#region Generate Method Overloads
	// Default color is red, and parent is world canvas.
	public static DamageText Generate(GameObject prefab, Vector3 pos, DamageTextStyle style, string textContent)
	{
		Transform canvas = GameObject.FindWithTag("WorldCanvas").transform;
		GameObject dmgTextObj = Instantiate(prefab, pos, Quaternion.identity);
		dmgTextObj.transform.SetParent(canvas, true);

		DamageText dmgText = dmgTextObj.GetComponentInChildren<DamageText>();

		Color textColor = style == DamageTextStyle.Critical ? CriticalColor : DamageColor;

		dmgText.Setup(textColor, textContent, style);
		return dmgText;
	}

	// Default parent is world canvas.
	public static DamageText Generate(GameObject prefab, Vector3 pos, Color txtColor, DamageTextStyle style, string textContent)
	{
		Transform canvas = GameObject.FindWithTag("WorldCanvas").transform;

		GameObject dmgTextObj = Instantiate(prefab, pos, Quaternion.identity);
		dmgTextObj.transform.SetParent(canvas, true);

		DamageText dmgText = dmgTextObj.GetComponentInChildren<DamageText>();

		dmgText.Setup(txtColor, textContent, style);
		return dmgText;
	}
	#endregion

	private void Setup(Color txtColor, string textContent, DamageTextStyle style)
	{
		transform.localScale = Vector3.zero;
		_currentTextColor = txtColor;
		_criticalHit = style == DamageTextStyle.Critical;

		displayText.text = textContent.ToUpper();
		displayText.color = _currentTextColor;
		displayText.fontSize = 1f;

		PopUp(style);
		GraduallyMoveUp();
	}

	#region Control Methods.
	private void GraduallyMoveUp()
	{
		float vel = _criticalHit ? maxVelocity * 1.5f : maxVelocity;
		transform.DOLocalMoveY(1f, vel).SetSpeedBased(true).SetEase(Ease.OutQuint).OnStart(PrepareForDestroying);
	}

	private void PopUp(DamageTextStyle style)
	{
		Tween popUpTween = transform.DOScale(_styleScales[style], .25f).SetEase(Ease.OutBack);

		if (_criticalHit)
		{
			popUpTween.OnComplete(() => CriticalTextIdling());
		}
	}

	private void CriticalTextIdling()
	{
		_idleSequence = DOTween.Sequence();

		_idleSequence.Append(transform.DOScale(.3f, .17f))
					 .Append(transform.DOScale(_styleScales[DamageTextStyle.Critical], .17f))
					 .SetLoops(-1, LoopType.Yoyo);
	}

	private void PrepareForDestroying()
	{
		Sequence sequence = DOTween.Sequence();

		sequence.AppendInterval(maxLifeTime)
				.Append(canvasGroup.DOFade(0f, .15f))
				.Join(transform.DOScale(0f, .2f))
				.SetEase(Ease.OutCubic)
				.AppendCallback(() => Destroying());
	}

	private void Destroying()
	{
		if (_idleSequence.IsActive())
			_idleSequence.Kill();
		
		Destroy(transform.parent.gameObject);
	}
	#endregion
}
