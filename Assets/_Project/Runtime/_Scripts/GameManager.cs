#region
using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Lumina.Essentials.Attributes;
using Lumina.Essentials.Modules;
using MelenitasDev.SoundsGood;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;
#endregion

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
	}

	[Header("Paused")]
	[SerializeField] bool isPaused;

	[Header("Boss")]
	[ReadOnly]
	[SerializeField] Boss currentBoss;

	[Header("State")]
	[ReadOnly]
	[SerializeField] State currentState = State.Lobby;
	[ReadOnly]
	[SerializeField] List<State> events = new ();

	public State GetNextEvent()
	{
		if (events.Count == 0) return State.Lobby;

		State nextEvent = events[0];
		events.RemoveAt(0);
		return nextEvent;
	}

	[Header("Debug")]
	[Tooltip("Just for fun. Allows you to spawn bosses without despawning the current one.")]
	[SerializeField] bool bypass;

	public Boss CurrentBoss
	{
		get
		{
			if (Helpers.FindMultiple<Boss>().Length > 1) return null; // this is only used for duo bosses, which aren't even supported
			return currentBoss;
		}
	}

	public State CurrentState => currentState;

	public bool IsPaused => isPaused;

	#region Pause & State Events
	public event Action<State> OnStateChanged;
	public event Action OnEnterLobby;
	public event Action OnEnterBattle;
	public event Action OnVictory;
	public event Action OnDefeat;
	public event Action OnEnterLoot;
	public event Action OnEnterShop;

	public event Action OnPause;
	public event Action OnResume;
	#endregion

	public void SetState(State state)
	{
		currentState = state;
		OnStateChanged?.Invoke(state);

		PlayerInputManager.instance.DisableJoining();

		switch (state)
		{
			case State.Lobby:
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
				SpawnBoss("Rem");

				AudioManager.StopMusic("LobbyMusic", 1f);

				var music = new Music(Track.BattleMusic);
				music.SetOutput(Output.Music);
				music.SetId("BattleMusic");
				music.SetVolume(0.5f);
				music.Play();

				OnEnterBattle?.Invoke();
				break;

			case State.Victory:
				AudioManager.StopMusic("BattleMusic", 1f);

				var victoryMusic = new Music(Track.VictoryMusic);
				victoryMusic.SetOutput(Output.Music);
				victoryMusic.SetVolume(0.5f);
				victoryMusic.SetId("VictoryMusic");
				victoryMusic.Play();

				OnVictory?.Invoke();
				break;

			case State.Defeat:
				OnDefeat += () =>
				{
					// AudioManager.StopAllMusic(1f);
					//
					// var defeatMusic = new Music(Track.DefeatMusic);
					// defeatMusic.SetOutput(Output.Music);
					// defeatMusic.SetVolume(0.5f);
					// defeatMusic.SetId("DefeatMusic");
					// defeatMusic.Play();
				};

				OnDefeat?.Invoke();
				break;

			case State.Loot:
				var lootMusic = new Music(Track.LootMusic);
				lootMusic.SetOutput(Output.Music);
				lootMusic.SetVolume(0.5f);
				lootMusic.SetId("LootMusic");
				lootMusic.Play();

				List<Item> items = Resources.LoadAll<Item>(AbilitySettings.ResourcePaths.ITEMS).ToList();
				Item item = items[Random.Range(0, items.Count)];

				foreach (Player player in PlayerManager.Instance.Players) player.Inventory.AddToInventory(item);

				OnEnterLoot?.Invoke();
				break;

			case State.Shop:

				#region Temporary until Shop implemented.
				List<Item> shopItems = Resources.LoadAll<Item>(AbilitySettings.ResourcePaths.ITEMS).ToList();
				Item shopItem = shopItems[Random.Range(0, shopItems.Count)];

				foreach (Player player in PlayerManager.Instance.Players) player.Inventory.AddToInventory(shopItem);
				Logger.LogWarning("The shop is not implemented yet. For now it just gives you a random item.");
				#endregion
				
				OnEnterShop?.Invoke();
				break;
		}
	}

	public event Action<Boss> OnBossSpawned;

	public void Initialize(Boss boss)
	{
		currentBoss = boss;
		OnBossSpawned?.Invoke(boss);

		currentBoss.OnBossStarted += () =>
		{
			DarkenBackground();
			StartTimer(currentBoss.AccentColour);
		};

		currentBoss.OnDeath += () =>
		{
			SetState(State.Victory);

			DarkenBackground(true);
			StopTimer();
		};
	}

	static void DarkenBackground(bool lighten = false)
	{
		GameObject background = GameObject.Find("Parallax");

		const float darkenValue = 20f / 255f;

		foreach (Transform child in background.transform) child.GetComponent<SpriteRenderer>().DOFade(lighten ? 1f : darkenValue, 1f).SetEase(Ease.OutCubic);
	}

	void StartTimer(Color color)
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
		};

		SetState(GetNextEvent());
	}

	void Update()
	{
		if (isTimerRunning) timer += Time.deltaTime;

#if UNITY_EDITOR
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			isPaused = !isPaused;

			const float maxCutoffFreq = 22000f;
			const float cutoffFreq = 1000f;
			AudioManager.Mixer.DOSetFloat("Muffler", isPaused ? cutoffFreq : maxCutoffFreq, 0.5f).SetUpdate(true);

			if (isPaused)
			{
				Time.timeScale = 0f;
				OnPause?.Invoke();
			}
			else
			{
				Time.timeScale = 1f;
				OnResume?.Invoke();
			}
		}
#endif
	}

#if UNITY_EDITOR
	void OnGUI() => // Display the timer in the top right corner
			GUI.Label(new (Screen.width - 200, 75, 200, 20), $"Time: {timer:F2}");

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
		
	}
#endif
}
