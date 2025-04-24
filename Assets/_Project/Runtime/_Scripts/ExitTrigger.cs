#region
using DG.Tweening;
using MelenitasDev.SoundsGood;
using UnityEngine;
using static Lumina.Essentials.Modules.Helpers;
#endregion

public class ExitTrigger : MonoBehaviour
{
	BoxCollider2D box;
	SpriteRenderer onwardGraphic;

	void Awake()
	{
		box = GetComponent<BoxCollider2D>();
		box.enabled = true;

		onwardGraphic = GetComponentInChildren<SpriteRenderer>();
	}

	void Start()
	{
		GameManager gameManager = GameManager.Instance;

		gameManager.OnBossSpawned += boss =>
		{
			box.enabled = false;
			boss.OnDeath += () => box.enabled = true;
		};

		gameManager.OnVictory += () => onwardGraphic.DOFade(1f, 0.35f);
		gameManager.OnEnterLobby += () => onwardGraphic.DOFade(1f, 0.35f);
		gameManager.OnEnterLoot += () => onwardGraphic.DOFade(1f, 0.35f);
		gameManager.OnEnterShop += () => onwardGraphic.DOFade(1f, 0.35f);
	}

	int temp;
	int playersInTrigger
	{
		get => temp = Mathf.Clamp(temp, 0, PlayerManager.PlayerCount);
		set => temp = value;
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.TryGetComponent(out Player _)) playersInTrigger++;

		if (playersInTrigger == PlayerManager.PlayerCount)
		{
			GameManager gameManager = GameManager.Instance;

			if (gameManager.CurrentBoss == null || gameManager.CurrentBoss.IsDead)
			{
				gameManager.SetState(GameManager.State.Transitioning);
				
				playersInTrigger = 0;
				
				AudioManager.StopAllMusic(1.85f);

				StageManager.ScrollLevel();

				onwardGraphic.DOFade(0f, 0.25f).OnComplete
						(() => CameraMain.transform.DOMoveX(CameraMain.transform.position.x + StageManager.STAGE_WIDTH, 2f).SetEase(Ease.InCirc).SetEase(Ease.OutCubic).SetUpdate(UpdateType.Late).OnComplete(() =>
						{
							gameManager.SetState(gameManager.GetNextEvent());
						}));
			}
		}
	}

	void OnTriggerExit2D(Collider2D other)
	{
		// check how many players are in the trigger
		if (other.TryGetComponent(out Player _)) playersInTrigger--;
	}
}
