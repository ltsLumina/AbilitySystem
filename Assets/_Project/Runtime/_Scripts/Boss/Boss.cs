#region
using System.Collections;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Lumina.Essentials.Modules;
using UnityEditor;
using UnityEngine;
using VInspector;
#endregion

[RequireComponent(typeof(Attacks))]
public class Boss : Entity
{
	[HideInInspector] [UsedImplicitly]
	public VInspectorData data;

	[Foldout("Sequence")]
	[SerializeField] List<Phase> phases = new ();
	[EndFoldout]
	[Header("Enrage Settings")]

	[SerializeField] float enrageDialogueDelay = 3f;
	[SerializeField] float enrageDelay = 5f;

	int currentPhaseIndex;

	Attacks attacks;

#if UNITY_EDITOR
	[Button] [UsedImplicitly]
	public void SaveList()
	{
		// Save the list of phases to a file
		string path = $"{Application.persistentDataPath}/{name}.json";
		string json = JsonUtility.ToJson(this, true);
		File.WriteAllText(path, json);
		Debug.Log($"Phases saved to {path}");
	}

	[Button] [UsedImplicitly]
	public void LoadList()
	{
		string path = EditorUtility.OpenFilePanel("Load Phases", Application.persistentDataPath, "json");
		if (string.IsNullOrEmpty(path)) return;
		string json = File.ReadAllText(path);
		JsonUtility.FromJsonOverwrite(json, this);
	}
#endif

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
		Logger.LogWarning("Enrage!");
		StartCoroutine(EnrageCoroutine());
	}

	IEnumerator EnrageCoroutine()
	{
		var dialogue = new Dialogue("I've had it with you!", enrageDialogueDelay + 2f);
		dialogue.type = Behaviour.Type.Dialogue;
		dialogue.Start(this);

		yield return new WaitForSeconds(enrageDialogueDelay);

		var move = new Move(Vector2.zero, 2f);
		move.type = Behaviour.Type.Move;
		move.Start(this);

		yield return new WaitForSeconds(enrageDelay);

		while (Helpers.Find<Player>().Health > 0)
		{
			attacks.Enrage();
			yield return new WaitForSeconds(enrageDelay);
		}

		Debug.LogWarning("Player has died. Ending enrage.");
	}
}
