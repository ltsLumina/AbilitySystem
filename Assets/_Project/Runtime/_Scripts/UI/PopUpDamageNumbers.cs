#region
using DG.Tweening;
using TMPro;
using UnityEngine;
#endregion

public static class PopUpDamageNumbers
{
	public static void ShowDamage(float damage, Vector3 position)
	{
		GameObject canvas = GameObject.FindWithTag("Worldspace Canvas");

		var damageNumberPrefab = Resources.Load<TextMeshProUGUI>("PREFABS/UI/Damage Number");
		TextMeshProUGUI instantiate = Object.Instantiate(damageNumberPrefab, position, Quaternion.identity, canvas.transform);

		float scaleFactor = damage switch
		{ >= 1000 => 1.5f,
		  >= 500  => 1.25f,
		  < 100   => 0.85f,
		  _       => 1f };

		instantiate.transform.localScale = Vector3.one * scaleFactor;
		
		Sequence sequence = DOTween.Sequence();
		sequence.Append(instantiate.transform.DOMoveY(position.y + 3f, 2f).SetEase(Ease.OutQuad));
		sequence.Append(instantiate.DOFade(0, 1f).SetEase(Ease.OutQuad));
		sequence.OnComplete(() => Object.Destroy(instantiate.gameObject));

		string msg = damage > 0 ? damage.ToString("F0") : "MISSED";
		instantiate.text = msg;
	}
}
