#region
using Lumina.Essentials.Attributes;
using Lumina.Essentials.Modules;
using MelenitasDev.SoundsGood;
using UnityEngine;
#endregion

public class GameManager : Singleton<GameManager>
{
	[ReadOnly]
	[SerializeField] Boss currentBoss;

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

	public void Initialize(Boss boss)
	{
		currentBoss = boss;
		currentBoss.OnBossStarted += StartTimer;
		currentBoss.OnDeath += StopTimer;

		AudioManager.StopMusic("LobbyMusic", 1f);

		var music = new Music(Track.BattleMusic);
		music.SetVolume(0.5f);
		music.SetFadeOut(1f);
		music.SetOutput(Output.Music);
		music.Play();
	}

	void StartTimer(Color color)
	{
		timer = 0f;
		isTimerRunning = true;
	}

	void StopTimer() => isTimerRunning = false;

	float timer;
	bool isTimerRunning;

	void Start()
	{
		var lobbyMusic = new Music(Track.LobbyMusic);
		lobbyMusic.SetOutput(Output.Music);
		lobbyMusic.SetId("LobbyMusic");
		lobbyMusic.Play();
	}

	void Update()
	{
		if (isTimerRunning) timer += Time.deltaTime;

#if UNITY_EDITOR
		if (Input.GetKeyDown(KeyCode.F1)) SpawnBoss(0);
		if (Input.GetKeyDown(KeyCode.F2)) SpawnBoss(1);
		if (Input.GetKeyDown(KeyCode.F3)) SpawnBoss(2);
		if (Input.GetKeyDown(KeyCode.F4)) SpawnBoss(3);
		if (Input.GetKeyDown(KeyCode.F5)) SpawnBoss(4);
		if (Input.GetKeyDown(KeyCode.F6)) SpawnBoss(5);
		if (Input.GetKeyDown(KeyCode.F7)) SpawnBoss(6);
		if (Input.GetKeyDown(KeyCode.F8)) SpawnBoss(7);
		if (Input.GetKeyDown(KeyCode.F9)) SpawnBoss(8);
		if (Input.GetKeyDown(KeyCode.F10)) SpawnBoss(9);
#endif
	}

#if UNITY_EDITOR
	void OnGUI() => // Display the timer in the top right corner
			GUI.Label(new (Screen.width - 200, 75, 200, 20), $"Time: {timer:F2}");

	void SpawnBoss(int index)
	{
		Boss[] bosses = FindObjectsByType<Boss>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);

		if (index < 0 || index >= bosses.Length)
		{
			Debug.LogError($"Boss index {index} is out of range.");
			return;
		}

		Boss boss = bosses[index];

		if (boss == null)
		{
			Debug.LogError($"Boss at index {index} is null.");
			return;
		}

		if (!bypass)
		{
			if (currentBoss != null)
			{
				Debug.LogError("A boss is already spawned. Please despawn the current boss before spawning a new one.");
				return;
			}
		}

		boss.gameObject.SetActive(true);
		currentBoss = boss;
	}
#endif
}