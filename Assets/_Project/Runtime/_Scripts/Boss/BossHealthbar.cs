#region
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using static Lumina.Essentials.Modules.Helpers;
#endregion

public class BossHealthbar : MonoBehaviour
{
	[SerializeField] Image progress;

	Boss boss;

	void Start() => boss = Find<Boss>();

	void Update() => progress.DOFillAmount(boss.Health / (float) boss.MaxHealth, 0.1f);
}
