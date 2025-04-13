#region
using System;
using DG.Tweening;
using Lumina.Essentials.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
#endregion

public class PlayerManager : Singleton<PlayerManager>
{
	[ReadOnly]
	[SerializeField] Player[] players = new Player[3];

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

		var hotbar = GameObject.Find($"Player {input.playerIndex + 1} Hotbar").GetComponent<Canvas>();

		if (hotbar)
		{
			hotbar.GetComponent<CanvasGroup>().alpha = 0f;
			hotbar.GetComponent<CanvasGroup>().DOFade(1f, 0.5f);
			hotbar.enabled = true;
		}

		OnPlayerJoined?.Invoke(player);
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
