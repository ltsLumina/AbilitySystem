using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using MelenitasDev.SoundsGood;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
	[Header("Paused?")]
	[SerializeField] bool isPaused;

	public bool IsPaused => isPaused;

	public static event Action OnPause;
	public static event Action OnResume;
	
	public void TogglePause()
	{
		var disallowedPauseStates = new List<GameManager.State>
		{ GameManager.State.Defeat,
		  GameManager.State.Transitioning };

		if (disallowedPauseStates.Contains(GameManager.Instance.CurrentState))
		{
			Debug.LogWarning($"Cannot pause in state: \"{GameManager.Instance.CurrentState}\".");
			return;
		}

		isPaused = !isPaused;

		const float maxCutoffFreq = 22000f;
		const float cutoffFreq = 1000f;
		const float duration = 0.5f;
		AudioManager.Mixer.DOSetFloat("Muffler", isPaused ? cutoffFreq : maxCutoffFreq, duration).SetUpdate(true);

		var pausableObjects = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID).OfType<IPausable>();

		if (isPaused)
		{
			foreach (IPausable pausable in pausableObjects) pausable.Pause();

			Time.timeScale = 0f;
			OnPause?.Invoke();
		}
		else
		{
			foreach (IPausable pausable in pausableObjects) pausable.Resume();

			Time.timeScale = 1f;
			OnResume?.Invoke();
		}
	}
}
