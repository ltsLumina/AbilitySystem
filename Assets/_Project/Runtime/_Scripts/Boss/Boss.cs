#region
using System.Collections;
using System.Collections.Generic;
using Lumina.Essentials.Modules;
using UnityEngine;
using VInspector;
#endregion

[RequireComponent(typeof(Attacks))]
public class Boss : Entity
{
	[HideInInspector]
	public VInspectorData data;

	[Foldout("Sequence")]
	[SerializeField] List<Phase> phases = new ();

	int currentPhaseIndex;

	Attacks attacks;

	void Awake() => attacks = GetComponent<Attacks>();

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

	protected override void OnStart()
	{
		Debug.Log("Boss started.");

		if (phases.Count > 0) StartPhase(phases[currentPhaseIndex]);
	}

	void StartPhase(Phase phase)
	{
		phase.OnPhaseEnded += HandlePhaseEnded;
		phase.Start(this);

		Debug.Log($"Starting phase \"{currentPhaseIndex + 1}\"");
	}

	void HandlePhaseEnded(Phase phase)
	{
		phase.OnPhaseEnded -= HandlePhaseEnded;
		currentPhaseIndex++;

		if (currentPhaseIndex < phases.Count) StartPhase(phases[currentPhaseIndex]);
		else
		{
			Debug.Log("All phases completed.");
			Enrage();
		}
	}

	void Enrage()
	{
		Debug.LogWarning("Enrage!");
		StartCoroutine(CorEnrage());
	}

	IEnumerator CorEnrage()
	{
		while (Helpers.Find<Player>().Health > 0 || Helpers.Find<Player>().CanRevive)
		{
			attacks.Enrage();
			yield return new WaitForSeconds(10f);
		}

		Debug.LogWarning("Player has died. Ending enrage.");
	}
}
