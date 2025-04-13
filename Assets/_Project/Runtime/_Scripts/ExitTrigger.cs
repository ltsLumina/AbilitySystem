#region
using DG.Tweening;
using UnityEngine;
using static Lumina.Essentials.Modules.Helpers;
#endregion

public class ExitTrigger : MonoBehaviour
{
	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.TryGetComponent(out Player player))
		{
			// if the boss is dead, we can exit
			if (GameManager.Instance.CurrentBoss == null || GameManager.Instance.CurrentBoss.Health <= 0)
			{
				StageManager.ScrollLevel();
				CameraMain.transform.DOMoveX(CameraMain.transform.position.x + 35, 2.5f).SetEase(Ease.InCirc).SetEase(Ease.OutCubic);
			}
		}
	}
}
