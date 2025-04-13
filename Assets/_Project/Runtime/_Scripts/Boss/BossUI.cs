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
		if (accentColour == Color.black || accentColour == Color.clear) accentColour = Random.ColorHSV(0, 1, 1, 1, 1, 1, 1, 1);
		
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

		OnTookDamage += dmg =>
		{
			if (!bossUI) return;
			bossUI.GetComponentInChildren<BossHealthbar>().TookDamage();
		};

		OnDeath += () => Destroy(bossUI, 1f);
	}

	void TerminateBossUI()
	{
		Transform canvas = GameObject.FindWithTag("Boss Canvas").transform;
		var fader = canvas.GetComponent<CanvasGroup>();

		Sequence sequence = DOTween.Sequence();
		sequence.Append(fader.DOFade(0f, 0.5f).SetEase(Ease.OutCubic));
	}
	#endregion

	void OnGUI()
	{
		if (statusEffects.Count == 0) return;

		// on the right of the middle of the screen
		GUILayout.BeginArea(new (Screen.width - 200, Screen.height / 2f - 100, 200, 200));

		foreach (StatusEffect effect in statusEffects)
		{
			if (effect == null) continue;

			GUILayout.Label($"{effect.StatusName} - {effect.Time.RoundTo(1)}");
		}

		GUILayout.EndArea();
	}
}
