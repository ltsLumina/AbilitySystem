#region
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
#endregion

public class BossHealthbar : MonoBehaviour
{
	[SerializeField] Image progress;

	Boss associatedBoss;
	Sequence sequence;

	public void Init(Boss boss) => associatedBoss = boss;

	public void TookDamage() => progress.DOFillAmount(associatedBoss.Health / (float) associatedBoss.MaxHealth, 0.1f).SetLink(associatedBoss.gameObject);
}
