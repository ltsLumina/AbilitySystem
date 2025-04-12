#region
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using JetBrains.Annotations;
using Lumina.Essentials.Modules;
using TMPro;
using UnityEditor;
using UnityEngine;
using VInspector;
using Random = UnityEngine.Random;
#endregion

[RequireComponent(typeof(Attacks))]
public class Boss : Entity
{
	[HideInInspector] [UsedImplicitly]
	public VInspectorData data;

	[Header("Health")]

	[SerializeField] int health = 10000;
	[SerializeField] int maxHealth = 10000;

	[Header("Visual")]

	[ColorUsage(false)]
	[SerializeField] Color accentColour = Color.black;

	[Foldout("Sequence")]
	[SerializeField] List<Phase> phases = new ();
	[EndFoldout]
	[Header("Enrage Settings")]

	[SerializeField] float enrageDialogueDelay = 3f;
	[SerializeField] float enrageDelay = 5f;

	int currentPhaseIndex;
	Attacks attacks;

	public int Health => health;
	public int MaxHealth => maxHealth;

	public event Action<Color> OnBossStarted;
	public event Action OnDeath;

	public Color AccentColour
	{
		get
		{
			accentColour.a = 1f;
			return accentColour;
		}
	}

#if UNITY_EDITOR
	[Button] [UsedImplicitly]
	public void SaveList()
	{
		// Save the list of phases to a file
		string path = $"{Application.persistentDataPath}/{name}.json";
		string json = JsonUtility.ToJson(this, true);
		File.WriteAllText(path, json);
		Debug.Log($"Phases saved to {path}");

		if (EditorUtility.DisplayDialog("Open File", "Do you want to open the file?", "Yes", "No")) Application.OpenURL($"file:///{path}");
		else EditorUtility.DisplayDialog("File Saved", $"Phases saved to {path}", "OK");
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

	public override void TakeDamage(float damage)
	{
		health -= Mathf.Clamp((int) damage, 0, health);
		if (health <= 0) OnDeath?.Invoke();
	}

	void Death()
	{
		Logger.LogWarning("Boss has died.");

		StopAllCoroutines();
		DOTween.Kill(this);

		attacks.StopAllCoroutines();
		DOTween.Kill(attacks);

		foreach (GameObject marker in GameObject.FindGameObjectsWithTag("Marker"))
		{
			if (marker == null) continue;
			DOTween.Kill(marker);
			Destroy(marker);
		}

		Sequence sequence = DOTween.Sequence();

		sequence.Append(transform.DOMoveY(2f, 0.5f).SetEase(Ease.OutQuad))      // Move the boss upward slightly
		        .Join(transform.DOMoveY(-10f, 1f).SetEase(Ease.InBack))         // Move the boss downward off-screen
		        .Join(transform.DOScale(Vector3.zero, 1f).SetEase(Ease.InBack)) // Shrink the boss as it falls
		        .OnComplete
		         (() =>
		         {
			         TerminateBossUI();
			         Destroy(gameObject, 1f);
		         });
	}

	void Awake()
	{
		attacks = GetComponent<Attacks>();

		OnDeath += Death;

		if (accentColour == Color.black || accentColour == Color.clear)
		{
			Debug.LogError("Accent colour is not set. Temporarily setting it to a random colour.", this);
			accentColour = Random.ColorHSV(0, 1, 1, 1, 0, 1, 1, 1);
		}

		GameManager.Instance.Initialize(this);
	}

	protected override void OnStart()
	{
		Debug.Log("Boss started.");

		if (phases.Count > 0)
		{
			StartPhase(phases[currentPhaseIndex]);
			OnBossStarted?.Invoke(AccentColour);
		}

		health = maxHealth;

		InitBossUI();
	}

	#region Boss UI
	void InitBossUI()
	{
		Transform canvas = GameObject.FindWithTag("Boss Canvas").transform;
		var fader = canvas.GetComponent<CanvasGroup>();

		Sequence sequence = DOTween.Sequence();
		sequence.OnStart(() => fader.alpha = 0);
		sequence.Append(fader.DOFade(1f, 0.5f).SetEase(Ease.OutCubic));

		var bossUIPrefab = Resources.Load<GameObject>("PREFABS/UI/Boss UI");
		GameObject bossUI = Instantiate(bossUIPrefab, canvas);
		bossUI.name = $"{name} UI";
		bossUI.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = name;
	}

	void TerminateBossUI()
	{
		Transform canvas = GameObject.FindWithTag("Boss Canvas").transform;
		var fader = canvas.GetComponent<CanvasGroup>();

		Sequence sequence = DOTween.Sequence();
		sequence.Append(fader.DOFade(0f, 0.5f).SetEase(Ease.OutCubic));
	}
	#endregion

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
