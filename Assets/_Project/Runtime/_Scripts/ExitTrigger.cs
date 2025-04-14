#region
using DG.Tweening;
using MelenitasDev.SoundsGood;
using UnityEngine;
using static Lumina.Essentials.Modules.Helpers;
#endregion

public class ExitTrigger : MonoBehaviour
{
	BoxCollider2D box;

	void Awake()
	{
		box = GetComponent<BoxCollider2D>();
		box.enabled = true;
	}

	void Start() => GameManager.Instance.OnBossSpawned += boss =>
	{
		box.enabled = false;
		boss.OnDeath += () => box.enabled = true;
	};

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.TryGetComponent(out Player _))
		{
			// if the boss is null or dead, we can exit
			GameManager gameManager = GameManager.Instance;

			if (gameManager.CurrentBoss == null || gameManager.CurrentBoss.Health <= 0)
			{
				//AudioManager.StopMusic("VictoryMusic", 2.5f, false);
				AudioManager.StopAllMusic(2.35f);
				
				StageManager.ScrollLevel();

				CameraMain.transform.DOMoveX(CameraMain.transform.position.x + StageManager.STAGE_WIDTH, 2.5f).SetEase(Ease.InCirc).SetEase(Ease.OutCubic).SetUpdate(UpdateType.Late).OnComplete(() => { gameManager.SetState(gameManager.GetNextEvent()); });
			}
		}
	}
}
