#region
using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Lumina.Essentials.Attributes;
using MelenitasDev.SoundsGood;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;
#endregion

public interface IPausable
{
	void Pause();

	void Resume();
}

public class GameManager : Singleton<GameManager>
{
	[Serializable]
	public enum State
	{
		Lobby,
		Battle,
		Victory,
		Defeat,
		Loot,
		Shop,
		Transitioning
	}
	
	[Header("Boss")]
	[ReadOnly]
	[SerializeField] Boss currentBoss;

	[Header("State")]
	[ReadOnly]
	[SerializeField] State currentState = State.Lobby;
	[ReadOnly]
	[SerializeField] List<State> events = new ();

	public List<State> Events => events;

	public Boss CurrentBoss => currentBoss;
	public bool IsTransitioning => currentState == State.Transitioning;

	public bool IsPaused => PauseManager.IsPaused;
	public PauseManager PauseManager { get; private set; }

	#region Pause & State Events
	public event Action<State> OnStateChanged;
	public event Action OnEnterLobby;
	public event Action OnEnterBattle;
	public event Action OnVictory;
	public event Action OnDefeat;
	public event Action OnEnterLoot;
	public event Action OnEnterShop;
	public event Action Transitioning;
	#endregion

	public State CurrentState => currentState;
	
	public State GetNextEvent()
	{
		if (events.Count == 0) return State.Lobby;

		State nextEvent = events[0];
		events.RemoveAt(0);
		return nextEvent;
	}
	
	int bossIndex = 0;

	public void SetState(State state)
	{
		currentState = state;
		OnStateChanged?.Invoke(state);

		PlayerInputManager.instance.DisableJoining();

		switch (state)
		{
			case State.Lobby:
				PauseManager.Resume();
				PlayerInputManager.instance.EnableJoining();

				AudioManager.StopMusic("VictoryMusic", 1f, false);

				var lobbyMusic = new Music(Track.LobbyMusic);
				lobbyMusic.SetOutput(Output.Music);
				lobbyMusic.SetVolume(0.5f);
				lobbyMusic.SetId("LobbyMusic");
				lobbyMusic.Play();

				OnEnterLobby?.Invoke();
				break;

			case State.Battle:
				switch (bossIndex)
				{
					case 0:
						SpawnBoss("Moog");
						break;
					case 1:
						SpawnBoss("Rem");
						break;
					case 2:
						SpawnBoss("Dragon-king Thordan");
						break;
					
					case 3:
						bossIndex = 0;
						SpawnBoss("Rem");
						break;
					
					default:
						SpawnBoss();
						break;
				}
				bossIndex++;

				AudioManager.StopMusic("LobbyMusic", 1f, false);

				var music = new Music(Track.BattleMusic);
				music.SetOutput(Output.Music);
				music.SetId("BattleMusic");
				music.SetVolume(0.5f);
				music.Play();

				OnEnterBattle?.Invoke();
				break;

			case State.Victory:
				AudioManager.StopMusic("BattleMusic", 1f, false);

				var victoryMusic = new Music(Track.VictoryMusic);
				victoryMusic.SetOutput(Output.Music);
				victoryMusic.SetVolume(0.5f);
				victoryMusic.SetId("VictoryMusic");
				victoryMusic.Play();

				OnVictory?.Invoke();
				break;

			case State.Defeat:
				PauseManager.Pause();
				
				OnDefeat?.Invoke();
				break;

			case State.Loot:
				var lootMusic = new Music(Track.LootMusic);
				lootMusic.SetOutput(Output.Music);
				lootMusic.SetVolume(0.5f);
				lootMusic.SetId("LootMusic");
				lootMusic.Play();
				
				var itemDistributor = FindAnyObjectByType<ItemDistributor>();
				itemDistributor?.Initialize();

				OnEnterLoot?.Invoke();
				break;

			case State.Shop:

				// #region Temporary until Shop implemented.
				// List<Item> shopItems = Resources.LoadAll<Item>(AbilitySettings.ResourcePaths.ITEMS).ToList();
				// Item shopItem = shopItems[Random.Range(0, shopItems.Count)];
				//
				// foreach (Player player in PlayerManager.Instance.Players) player.Inventory.AddToInventory(shopItem);
				// Logger.LogWarning("The shop is not implemented yet. For now it just gives you a random item.");
				// #endregion
				
				SetState(State.Loot);

				OnEnterShop?.Invoke();
				break;
			
			case State.Transitioning:
				// blank state only used for transitioning
				Transitioning?.Invoke();
			break;

			default:
				throw new ArgumentOutOfRangeException(nameof(state), state, null);
		}
	}

	public event Action<Boss> OnBossSpawned;

	/// <summary>
	///     Called when a boss is spawned and initializes the GameManager's state with the boss.
	/// </summary>
	/// <remarks> The game will not work properly if this method is not called. </remarks>
	/// <param name="boss"> The boss to begin combat with. </param>
	public void InitializeState(Boss boss)
	{
		currentBoss = boss;
		OnBossSpawned?.Invoke(boss);

		currentBoss.OnBossStarted += () =>
		{
			FadeOutBackground();
			StartTimer();
		};

		currentBoss.OnDeath += () =>
		{
			// Checks if the player(s) die before the boss.
			if (PlayerManager.Instance.Players.All(p => p.IsDead)) 
				SetState(State.Defeat);
			else 
				SetState(State.Victory);

			FadeInBackground();
			StopTimer();
		};
	}

	public static void FadeOutBackground(float duration = 1f)
	{
		GameObject background = GameObject.Find("Parallax");

		const float darkenValue = 20f / 255f;

		foreach (Transform child in background.transform) child.GetComponent<SpriteRenderer>().DOFade(darkenValue, duration).SetEase(Ease.OutCubic);
	}

	public static void FadeInBackground(float duration = 1f)
	{
		GameObject background = GameObject.Find("Parallax");

		const float lightenValue = 1f;

		foreach (Transform child in background.transform) child.GetComponent<SpriteRenderer>().DOFade(lightenValue, duration).SetEase(Ease.OutCubic);
	}

	void StartTimer()
	{
		timer = 0f;
		isTimerRunning = true;
	}

	void StopTimer() => isTimerRunning = false;

	float timer;
	bool isTimerRunning;

	protected override void OnDestroy()
	{
		base.OnDestroy();

		StageManager.Reset();
	}
	
	void Start()
	{
		events.Clear();

		OnEnterLobby += () =>
		{
			var allEvents = new List<State>
			{ State.Battle,
			  State.Loot,
			  State.Battle,
			  State.Shop,
			  State.Battle };

			events.AddRange(allEvents);

			if (events.Count != allEvents.Count) 
				Debug.LogWarning("The events list is longer than the expected. This may cause unexpected behavior.");
		};

		SetState(GetNextEvent());
	}

	protected override void Awake()
	{
		base.Awake();

		PauseManager = GetComponent<PauseManager>();
	}

	void Update()
	{
		if (isTimerRunning) timer += Time.deltaTime;

#if UNITY_EDITOR
		if (Input.GetKeyDown(KeyCode.F1))
		{
			CurrentBoss.Kill();
		}
#endif
	}
	
#if UNITY_EDITOR
	void OnGUI()
	{
		// Display the timer in the top right corner
		GUI.Label(new (Screen.width - 200, 75, 200, 20), $"Time: {timer:F2}");

		GUILayout.BeginArea(new Rect(0, 200, 100, 100));

		bool killBoss = CurrentBoss != null && !CurrentBoss.IsDead;
		bool spawnBoss = CurrentBoss == null || CurrentBoss.IsDead;
		string title = CurrentBoss ? $"Kill {CurrentBoss.ShortName}" : "Spawn Boss";

		if (GUILayout.Button(title))
		{
			if (killBoss)
			{
				CurrentBoss.Kill();
			}
			else if (spawnBoss)
			{
				SpawnBoss();
			}
		}

		if (GUILayout.Button("Skip to Loot"))
		{
			SetState(State.Loot);
		}

		GUILayout.EndArea();
	}
#endif

	/// <summary>
	///     Randomly selects a boss from the list of bosses and spawns it at the origin position.
	/// </summary>
	/// <remarks>
	///     This method loads all boss prefabs from the "PREFABS/Bosses" resource folder,
	///     randomly selects one, and instantiates it at position (0,0,0) with default rotation.
	///     If no boss prefabs are found, it logs an error and returns without spawning.
	/// </remarks>
	void SpawnBoss()
	{
		// randomly select a boss from the list of bosses
		List<Boss> bosses = Resources.LoadAll<Boss>("PREFABS/Bosses").ToList();

		Boss boss = bosses[Random.Range(0, bosses.Count)];

		if (boss == null)
		{
			Debug.LogError("Boss not found");
			return;
		}

		Boss instance = Instantiate(boss, Vector3.zero, Quaternion.identity);
	}

	/// <summary>
	///     Spawns a specific boss by searching for a boss prefab that starts with the given name.
	/// </summary>
	/// <param name="shortName">The prefix name of the boss to spawn. The name is case-sensitive.</param>
	/// <remarks>
	///     This method loads all boss prefabs from the "PREFABS/Bosses" resource folder,
	///     finds the first boss whose name starts with the provided shortName parameter,
	///     and instantiates it at position (0,0,0) with default rotation.
	///     If no matching boss is found, it logs an error and returns without spawning.
	/// </remarks>
	/// <example> "Rem, the Sooteared Wolf" can be spawned with input "Rem" </example>
	void SpawnBoss(string shortName)
	{
		List<Boss> bosses = Resources.LoadAll<Boss>("PREFABS/Bosses").ToList();

		// trim everything after the first comma
		Boss boss = bosses.FirstOrDefault(b => b.name.StartsWith(shortName));
		if (boss == null)
		{
			Debug.LogError($"Boss {shortName} not found");
			return;
		}

		Boss instance = Instantiate(boss, Vector3.zero, Quaternion.identity);
	}

	void SpawnScarecrow()
	{
		var scarecrow = Resources.Load<Boss>("PREFABS/Bosses/Scarecrow");

		if (scarecrow == null)
		{
			Debug.LogError("Scarecrow not found");
			return;
		}

		Boss instance = Instantiate(scarecrow, Vector3.zero, Quaternion.identity);
		instance.gameObject.SetActive(true);
	}

	// TODO: replace this
	public void RestartGame() => SceneManagerExtended.ReloadScene();
}