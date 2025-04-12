#region
using System;
using Lumina.Essentials.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
#endregion

public class PlayerManager : Singleton<PlayerManager>
{
	[ReadOnly]
	[SerializeField] Player[] players = new Player[4];

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

	void Start()
	{
		var manager = PlayerInputManager.instance;

		manager.onPlayerJoined += HandlePlayerJoined;
		manager.onPlayerLeft += HandlePlayerLeft;
	}

	void HandlePlayerJoined(PlayerInput input)
	{
		var player = input.GetComponentInParent<Player>();
		AddPlayer(player);

		player.name = $"Player {input.playerIndex + 1}";
		player.tag = $"Player {input.playerIndex + 1}";
		player.transform.SetParent(GameObject.Find("Important").transform);

		//InitializeUI();

		var hotbar = GameObject.Find($"Player {input.playerIndex + 1} Hotbar").GetComponent<Canvas>();
		if (hotbar != null) hotbar.enabled = true;

		OnPlayerJoined?.Invoke(player);

		// return;
		// void InitializeUI()
		// {
		// 	var hotbarCanvas = Resources.Load<GameObject>("PREFABS/UI/Hotbar Canvas");
		// 	GameObject canvas = Instantiate(hotbarCanvas, GameObject.Find("UI").transform);
		// 	canvas.name = $"Player {input.playerIndex + 1} Hotbar Canvas";
		//
		// 	var hotbar = canvas.transform.GetChild(0).transform as RectTransform;
		// 	
		// 	AbilityButton[] abilityButtons = hotbar.GetComponentsInChildren<AbilityButton>();
		// 	foreach (AbilityButton button in abilityButtons)
		// 	{
		// 		button.tag = $"Player {input.playerIndex + 1}";
		// 	}
		// }
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
