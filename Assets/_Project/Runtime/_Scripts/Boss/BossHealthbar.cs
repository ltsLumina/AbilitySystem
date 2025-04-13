#region
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
#endregion

public class BossHealthbar : MonoBehaviour
{
	[SerializeField] Image progress;

	Boss associatedBoss;

	public void Init(Boss boss) => associatedBoss = boss;

	void Update()
	{
		if (associatedBoss == null) return;
		Sequence sequence = DOTween.Sequence();
		sequence.Append(progress.DOFillAmount(associatedBoss.Health / (float) associatedBoss.MaxHealth, 0.1f));
		sequence.SetLink(associatedBoss.gameObject);
	}
}
