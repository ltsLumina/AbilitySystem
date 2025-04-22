#region
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using VInspector;
using static Lumina.Essentials.Modules.Helpers;
#endregion

[RequireComponent(typeof(Attacks))]
public sealed partial class Boss : Entity
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
	[SerializeField] float enrageInterval = 5f;

	int currentPhaseIndex;
	Attacks attacks;

	public int Health
	{
		get => health;
		private set
		{
			health = Mathf.Clamp(value, 0, maxHealth);

			// QoL change: if health is at less than 50, set health to zero.
			// Reduces the possibility of the player dying when the boss has a tiny amount of health left.
			const int THRESHOLD = 50;
			if (health <= THRESHOLD) health = 0;

			if (health <= 0)
			{
				if (IsDead) return;

				OnDeath?.Invoke();
				health = 0;
				IsDead = true;
			}
		}
	}
	public int MaxHealth => maxHealth;
	public bool IsDead { get; private set; }

	public event Action OnBossStarted;
	public event Action<int> OnTookDamage;
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
	public void Kill() => Health = 0;

	[Button] [UsedImplicitly]
	public void SaveList()
	{
		if (Application.isPlaying) return;
		
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
		if (Application.isPlaying) return;
		
		string path = EditorUtility.OpenFilePanel("Load Phases", Application.persistentDataPath, "json");
		if (string.IsNullOrEmpty(path)) return;
		string json = File.ReadAllText(path);
		JsonUtility.FromJsonOverwrite(json, this);
	}
#endif

	void Awake()
	{
		attacks = GetComponent<Attacks>();

		OnBossStarted += () => CameraMain.DOOrthoSize(CameraMain.orthographicSize * 1.10f, 1.5f);

		OnDeath += () =>
		{
			CameraMain.DOOrthoSize(CameraMain.orthographicSize * 0.9f, 1.5f);
			Death();
		};
	}
	
	protected override void OnStart()
	{
		if (!TryGetComponent<Scarecrow>(out _))
		{
			// only initialize the state if this is not a scarecrow
			// the scarecrow is only for the lobby and doesn't count as a boss. it does, however, still use the Boss class.
			GameManager.Instance.InitializeState(this);
		}

		if (phases.Count > 0)
		{
			StartPhase(phases[currentPhaseIndex]);
			OnBossStarted?.Invoke();
		}

		float adjustedHealth = Mathf.RoundToInt(maxHealth * HealthScalar);
		health = (int) adjustedHealth;

		name = name.Replace("(Clone)", string.Empty);
		transform.SetParent(GameObject.Find("Important").transform);
		transform.SetAsLastSibling();

		InitBossUI();
	}

	#region Health / Take Damage
	float HealthScalar
	{
		get
		{
			int players = PlayerManager.Instance.Players.Count;
			if (players == 0) players = 1;

			// each player adds 25% health to the boss
			float scalar = 1f + (players - 1) * 0.25f;

			return scalar;
		}
	}

	public override void TakeDamage(float damage)
	{
		if (IsDead) return;
		
		int dmg = Mathf.Clamp((int) damage, 0, health);
		Health -= dmg;
		OnTookDamage?.Invoke(dmg);

		if (TryGetComponent(out Scarecrow dummy)) dummy?.RegisterDamage(dmg);

		PopUpDamageNumbers.ShowDamage(damage, transform.position);
	}

	void Death()
	{
		Logger.LogWarning("Boss has died.");
		Destroy(gameObject, 5f);

		StopAllCoroutines();
		DOTween.Kill(this);

		attacks.StopAllCoroutines();
		DOTween.Kill(attacks);

		foreach (GameObject marker in GameObject.FindGameObjectsWithTag("Marker"))
		{
			if (marker == null) continue;
			DOTween.Kill(marker);
			marker.GetComponentInChildren<SpriteRenderer>()?.DOFade(0, 0.5f);
			Destroy(marker, 1f);
		}

		foreach (Orb orb in FindObjectsByType<Orb>(FindObjectsSortMode.None)) Destroy(orb.gameObject);

		TerminateBossUI();

		var rb = gameObject.AddComponent<Rigidbody2D>();
		rb.bodyType = RigidbodyType2D.Dynamic;
		DOTween.Kill("Move");
		const float FORCE = 1.5f;
		rb.AddForce(new Vector2(2, 10f) * FORCE, ForceMode2D.Impulse);
		rb.AddTorque(-120f, ForceMode2D.Impulse);
	}
	#endregion

	#region Phase Management
	void StartPhase(Phase phase)
	{
		phase.OnPhaseEnded += HandlePhaseEnded;
		phase.Start(this);

		Logger.Log($"Starting phase: \"{phase.Name}\"");
	}

	void HandlePhaseEnded(Phase phase)
	{
		phase.OnPhaseEnded -= HandlePhaseEnded;
		currentPhaseIndex++;

		if (currentPhaseIndex < phases.Count) StartPhase(phases[currentPhaseIndex]);
		else
		{
			Debug.Log("All phases completed.");
			StartCoroutine(Enrage());
		}
	}

	IEnumerator Enrage()
	{
		Logger.LogWarning("Enrage!");

		var dialogue = new Dialogue("I've had it with you!", enrageDialogueDelay);
		dialogue.type = Behaviour.Type.Dialogue;
		dialogue.Start(this);

		yield return new WaitForSeconds(enrageDialogueDelay);

		var move = new Move(Vector2.zero, 2f);
		move.type = Behaviour.Type.Move;
		move.Start(this);

		yield return new WaitForSeconds(2f);

		attacks.Enrage(enrageInterval);

	}
	#endregion
}
