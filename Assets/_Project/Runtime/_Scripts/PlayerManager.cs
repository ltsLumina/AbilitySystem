#region
using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Lumina.Essentials.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using Random = UnityEngine.Random;
#endregion

public class PlayerManager : Singleton<PlayerManager>
{
	[ReadOnly]
	[SerializeField] Player[] players = new Player[3];

	/// <summary>
	///     The players in the game.
	/// </summary>
	/// <returns>
	///     A read-only array of players. This array will not contain any null values.
	///     The count will therefore reflect how many players are actually playing.
	/// </returns>
	public ReadOnlyArray<Player> Players
	{
		get
		{
			// return players but strip the null values
			var nonNullPlayers = new Player[players.Length];
			int count = 0;

			foreach (Player p in players)
			{
				if (p == null) continue;
				nonNullPlayers[count] = p;
				count++;
			}

			var result = new Player[count];
			Array.Copy(nonNullPlayers, result, count);
			return new (result);
		}
	}
	
	// note: this could potentially be an issue because it is static
	public static int PlayerCount => Instance.Players.Count;

	public event Action<Player> OnPlayerJoined;
	public event Action<Player> OnPlayerLeft;

	/// <summary>
	///     Uses a zero-based index to get the player at the given index.
	///     <para>An index of 0 will return the first player, 1 will return the second player, etc.</para>
	/// </summary>
	public Player GetPlayer(int index)
	{
		if (index < 0 || index >= players.Length) return null;
		return players[index];
	}

	void AddPlayer(Player player)
	{
		// Check if the player is already in the array, if so, return. Otherwise, add the player to the first empty slot.
		for (int i = 0; i < players.Length; i++)
		{
			if (players[i] == null)
			{
				players[i] = player;
				return;
			}
		}

		Debug.LogWarning("No empty slots for new players.");
	}

	void RemovePlayer(Player player)
	{
		// Check if the player is in the array, if so, remove the player from the array.
		for (int i = 0; i < players.Length; i++)
		{
			if (players[i] == player)
			{
				players[i] = null;
				return;
			}
		}

		Debug.LogWarning("Player not found in the array.");
	}

#if UNITY_EDITOR
	void OnGUI()
	{
		List<GameObject> allJobs = Resources.LoadAll<GameObject>("PREFABS/Jobs").ToList();
		GameObject defaultJob = DefaultJob;

		GUILayout.BeginArea(new (0, 100, 100, 100));

		string prettyName = defaultJob.name.Replace("(default)", string.Empty);
		if (GUILayout.Button($"Spawn {prettyName}"))
		{
			PlayerInputManager.instance.playerPrefab = defaultJob;
			PlayerInputManager.instance.JoinPlayer();
		}

		foreach (GameObject job in allJobs)
		{
			if (job == defaultJob) continue;

			if (GUILayout.Button($"Spawn {job.name}"))
			{
				PlayerInputManager.instance.playerPrefab = job;
				PlayerInputManager.instance.JoinPlayer();
			}
		}

		GUILayout.EndArea();
	}
#endif

	void Start()
	{
		var manager = PlayerInputManager.instance;
		
		manager.onPlayerJoined += HandlePlayerJoined;
		manager.onPlayerLeft += HandlePlayerLeft;

		manager.playerPrefab = DefaultJob;
	}

	GameObject DefaultJob
	{
		get
		{
			List<GameObject> allJobs = Resources.LoadAll<GameObject>("PREFABS/Jobs").ToList();
#if UNITY_EDITOR
			GameObject defaultJob = allJobs.FirstOrDefault(x => x.name.Contains("default"));
#else
			var defaultJob = allJobs.FirstOrDefault(x => x.name.Contains("RPR"));
#endif

			if (defaultJob == null)
			{
				Logger.LogError("Default job not found");
				return null;
			}

			return defaultJob;
		}
	}

	void HandlePlayerJoined(PlayerInput input)
	{
		CycleControlScheme();
		
		var player = input.GetComponentInParent<Player>();
		AddPlayer(player);

		bool firstPlayer = input.playerIndex == 0;

		if (firstPlayer) player.transform.position = new (-10f, 0f, 0f);
		else player.transform.position = Players[0].transform.position + new Vector3(Random.insideUnitCircle.x, Random.insideUnitCircle.y) * 3f;

		player.tag = $"Player {player.ID}";
		player.transform.SetParent(GameObject.Find("Important").transform);
		player.transform.SetSiblingIndex(input.playerIndex);
		player.gameObject.name = player.name;

		var canvas = GameObject.Find($"Player {player.ID} Hotbar").GetComponent<Canvas>();
		if (canvas)
		{
			canvas.GetComponent<CanvasGroup>().alpha = 0f;
			canvas.GetComponent<CanvasGroup>().DOFade(1f, 0.5f);
			canvas.enabled = true;
		}

		OnPlayerJoined?.Invoke(player);

		return;
		void CycleControlScheme() => input.actions["Switch"].performed += _ =>
		{
			Gamepad[] gamepads = Gamepad.all.ToArray();
			Gamepad currentGamepad = Gamepad.current;
            
			if (input.currentControlScheme == "Keyboard&Mouse")
			{
				// If we're on keyboard, switch to first available gamepad
				if (gamepads.Length > 0)
				{
					input.SwitchCurrentControlScheme("Gamepad", gamepads[0]);
				}
			}
			else
			{
				// We're currently on a gamepad
				if (gamepads.Length > 1)
				{
					// Find current gamepad index
					int currentIndex = Array.IndexOf(gamepads, currentGamepad);
                    
					// If there's a next gamepad, switch to it
					if (currentIndex < gamepads.Length - 1)
					{
						input.SwitchCurrentControlScheme("Gamepad", gamepads[currentIndex + 1]);
						return;
					}
				}
                
				// If we're on the last gamepad or only have one gamepad, switch back to keyboard
				input.SwitchCurrentControlScheme("Keyboard&Mouse", Keyboard.current);
			}
		};
	}

	void HandlePlayerLeft(PlayerInput input)
	{
		var player = input.GetComponentInParent<Player>();
		RemovePlayer(player);

		// var hotbar = GameObject.FindWithTag($"Player {input.playerIndex + 1} Hotbar").GetComponent<Canvas>();
		// if (hotbar != null) hotbar.enabled = false;

		OnPlayerLeft?.Invoke(player);
	}
}