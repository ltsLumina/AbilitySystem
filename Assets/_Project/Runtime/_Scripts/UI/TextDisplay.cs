#region
using DG.Tweening;
using TMPro;
using UnityEngine;
#endregion

public static class TextDisplay
{
	/// <summary>
	/// Shows a damage number at the specified world space position.
	/// </summary>
	/// <remarks> This method uses world space position, not screen space position. </remarks>
	/// <param name="damage"></param>
	/// <param name="worldSpacePosition"></param>
	public static void ShowDamage(float damage, Vector3 worldSpacePosition)
	{
		GameObject canvas = GameObject.FindWithTag("Worldspace Canvas");

		var damageNumberPrefab = Resources.Load<TextMeshProUGUI>("PREFABS/UI/Damage Number");
		TextMeshProUGUI instantiate = Object.Instantiate(damageNumberPrefab, worldSpacePosition, Quaternion.identity, canvas.transform);

		float scaleFactor = damage switch
		{ >= 1000 => 1.5f,
		  >= 500  => 1.25f,
		  < 100   => 0.85f,
		  _       => 1f };

		instantiate.transform.localScale = Vector3.one * scaleFactor;
		
		Sequence sequence = DOTween.Sequence();
		sequence.Append(instantiate.transform.DOMoveY(worldSpacePosition.y + 3f, 2f).SetEase(Ease.OutQuad));
		sequence.Append(instantiate.DOFade(0, 1f).SetEase(Ease.OutQuad));
		sequence.OnComplete(() => Object.Destroy(instantiate.gameObject));

		string msg = damage > 0 ? damage.ToString("F0") : "MISSED";
		instantiate.text = msg;
	}

	/// <summary>
	///  Shows a dice roll number at the specified screen position.
	/// </summary>
	/// <remarks> This method uses screen space position, not world space position. </remarks>
	/// <param name="rollValue"> The value of the dice roll. </param>
	/// <param name="duration"></param>
	/// <param name="parent"></param>
	/// <returns></returns>
	public static TextMeshProUGUI ShowDiceRoll(float rollValue, float duration, Transform parent)
	{
		var damageNumberPrefab = Resources.Load<TextMeshProUGUI>("PREFABS/UI/Dice Number");
		TextMeshProUGUI instantiate = Object.Instantiate(damageNumberPrefab, parent);
		instantiate.transform.localScale = Vector3.one;
		
		Sequence sequence = DOTween.Sequence();
		sequence.Append(instantiate.transform.DOLocalMoveY(60, duration * 0.66f).SetEase(Ease.OutQuad));
		sequence.Append(instantiate.DOFade(0, duration * 0.33f).SetEase(Ease.OutSine));
		sequence.SetLink(instantiate.gameObject);
		sequence.OnComplete(() => Object.Destroy(instantiate.gameObject));
		
		instantiate.text = rollValue.ToString("F0");

		return instantiate;
	}
}
