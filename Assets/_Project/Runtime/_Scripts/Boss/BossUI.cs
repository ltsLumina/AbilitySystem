#region
using DG.Tweening;
using TMPro;
using UnityEngine;
#endregion

/// <summary>
///     BossUI.cs
/// </summary>
public partial class Boss
{
	#region Boss UI
	void InitBossUI()
	{
		Transform canvas = GameObject.FindWithTag("Boss Canvas").transform;
		var fader = canvas.GetComponent<CanvasGroup>();

		Sequence sequence = DOTween.Sequence();
		sequence.OnStart(() => fader.alpha = 0);
		sequence.Append(fader.DOFade(1f, 0.5f).SetEase(Ease.OutCubic));

		var bossUIPrefab = Resources.Load<GameObject>("PREFABS/UI/Boss UI");
		GameObject bossUI = Instantiate(bossUIPrefab, canvas);
		bossUI.name = $"{name} UI";
		bossUI.GetComponentInChildren<BossHealthbar>().Init(this);
		bossUI.GetComponentInChildren<Accentize>().Recolour(accentColour);
		bossUI.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = name;
	}

	void TerminateBossUI()
	{
		Transform canvas = GameObject.FindWithTag("Boss Canvas").transform;
		var fader = canvas.GetComponent<CanvasGroup>();

		Sequence sequence = DOTween.Sequence();
		sequence.Append(fader.DOFade(0f, 0.5f).SetEase(Ease.OutCubic));
	}
	#endregion
}
