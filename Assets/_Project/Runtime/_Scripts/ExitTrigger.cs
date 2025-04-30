#region
using System;
using DG.Tweening;
using MelenitasDev.SoundsGood;
using TMPro;
using UnityEngine;
using static Lumina.Essentials.Modules.Helpers;
#endregion

public class ExitTrigger : MonoBehaviour
{
	[Header("Text")]
	[SerializeField] TMP_Text onwardText;

	BoxCollider2D box;
	SpriteRenderer onwardGraphic;

	void Awake()
	{
		box = GetComponent<BoxCollider2D>();
		box.enabled = true;

		onwardGraphic = GetComponentInChildren<SpriteRenderer>();

		// Initialize text
		if (onwardText != null)
		{
			onwardText.alpha = 0f;
			onwardText.text = "0/1";
		}
	}

	void Start()
	{
		GameManager gameManager = GameManager.Instance;

		gameManager.OnStateChanged += state =>
		{
			switch (state)
			{
				case GameManager.State.Battle:
				case GameManager.State.Defeat:
					box.enabled = false;
					break;

				case GameManager.State.Lobby:
				case GameManager.State.Victory:
				case GameManager.State.Loot:
				case GameManager.State.Shop:
					ShowText();									
					box.enabled = true;
					onwardGraphic.DOFade(1, 0.35f);
					break;

				case GameManager.State.Transitioning:
					HideText();
					break;

				default:
					throw new ArgumentOutOfRangeException(nameof(state), state, null);
			}
		};
		
		ShowText();
	}

	void Update() => UpdateTextVisibility();

	int temp;
	int playersInTrigger
	{
		get => temp = Mathf.Clamp(temp, 0, PlayerManager.PlayerCount);
		set => temp = value;
	}

	void ShowText()
	{
		onwardText.gameObject.name = $"Onward | {UpdateTextVisibility()}";
		onwardText.DOFade(1f, 0.35f);
	}

	void HideText() => onwardText.DOFade(0f, 0.35f);

	string UpdateTextVisibility()
	{
		return onwardText.text = PlayerManager.PlayerCount > 0 ? $"{playersInTrigger}/{PlayerManager.PlayerCount}" : "waiting for players...";
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

				AudioManager.StopAllMusic(1.85f);

				StageManager.ScrollLevel();

				onwardGraphic.DOFade(0f, 0.25f)
				             .OnComplete
				              (() => CameraMain.transform.DOMoveX(CameraMain.transform.position.x + StageManager.STAGE_WIDTH, 2f)
				                               .SetEase(Ease.InCirc)
				                               .SetEase(Ease.OutCubic)
				                               .SetUpdate(UpdateType.Late)
				                               .OnComplete
				                                (() =>
				                                {
					                                onwardText.transform.position += Vector3.right * StageManager.STAGE_WIDTH;
					                                gameManager.SetState(gameManager.GetNextEvent());
					                                playersInTrigger = 0;
				                                }));
			}
		}
	}

	void OnTriggerExit2D(Collider2D other)
	{
		if (other.TryGetComponent(out Player _)) playersInTrigger--;
	}
}
