#region
using System.Collections.Generic;
using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using VInspector;
using static GameManager;
#endregion

public class Timeline : MonoBehaviour
{
	[SerializeField] List<Image> milestones;
	[SerializeField] Image progress;

	float startPosition;
	float targetPosition;

#if UNITY_EDITOR
	[Button] [UsedImplicitly]
	public void ResetUI() => transform.localPosition = new (transform.localPosition.x, transform.localPosition.y + 100, 0f);
#endif

	void Reset()
	{
		if (!progress) Debug.LogError("Progress bar not assigned in Timeline.");
		
		progress.fillAmount = 0f;
		foreach (var milestone in milestones)
		{
			milestone.enabled = true;
		}
		
		transform.localPosition = new (transform.localPosition.x, transform.localPosition.y + 100, 0f);
		index = -1;
	}

	void Start()
	{
		Singleton<GameManager>.Instance.OnStateChanged += HandleStateChange;
		
		transform.localPosition = new (transform.localPosition.x, transform.localPosition.y + 100, 0f);
		startPosition = transform.localPosition.y;
		targetPosition = transform.localPosition.y - 100f;
	}

	int index = -1;

	void HandleStateChange(State state)
	{
		if (state == State.Lobby)
		{
			Reset();
			return;
		}

		switch (state)
		{
			case State.Transitioning:
				transform.DOLocalMoveY(startPosition, 0.35f).SetEase(Ease.OutCubic).OnComplete(Tween);
				break;

			case State.Victory:
				// do nothing
				break;

			default: 
			{
				index++;

				for (int i = 0; i < milestones.Count; i++)
				{
					if (i == index)
						milestones[i].enabled = false;
				}

				if (state is State.Lobby or State.Loot or State.Shop)
				{
					transform.DOLocalMoveY(targetPosition, 0.35f).SetEase(Ease.OutCubic).OnComplete(Tween);
				}
				else
				{
					transform.DOLocalMoveY(startPosition, 0.35f).SetEase(Ease.OutCubic).OnComplete(Tween);
				}

				break;
			}
		}
	}
	
	void Tween()
	{
		// tweens the fill amount based on this function: y=0.165x+0.075
		float fillAmount = 0.165f * (index + 1) + 0.075f;
		progress.DOFillAmount(fillAmount, 0.85f).SetEase(Ease.OutCubic);
	}
}
