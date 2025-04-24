#define UNITY_ASSERTIONS

#region
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VInspector;
using Random = UnityEngine.Random;
#endregion

public class ItemDistributor : MonoBehaviour
{
	[SerializeField] SerializedDictionary<Player, SceneItem> playerVotes = new ();
	[SerializeField] List<Player> finishedPlayers = new ();

	readonly Dictionary<SceneItem, Player> results = new ();

	public event Action<SceneItem, Player> OnItemVoted;
	public event Action<SceneItem, Player> OnItemDistributed;
	public event Action<Dictionary<SceneItem, Player>> OnAllItemsDistributed;
	
	void Awake()
	{
		playerVotes.Clear();
		finishedPlayers.Clear();

		OnItemDistributed += (_, player) => player.InputManager.EventSystem.SetSelectedGameObject(null);
		OnAllItemsDistributed += DistributionComplete;
	}

	public void Initialize()
	{
		// Initialize the item distribution process
		List<SceneItem> availableItems = Resources.LoadAll<SceneItem>("PREFABS/Items").ToList();
		// Remove items that are null (includes the template item)
		availableItems.RemoveAll(item => !item.RepresentedItem);
		// Remove items that are already in the players' inventories.
		availableItems.RemoveAll(item => PlayerManager.Instance.Players.Any(player => player.Inventory.HasItem(item)));

#if UNITY_EDITOR && UNITY_ASSERTIONS
		const string message = "Not enough items available for distribution." 
		                       + "\nThere were only {0} items available, but {1} players are present." 
		                       + "\nThere must be at least an equal number of items as players.";
		Debug.AssertFormat(availableItems.Count > PlayerManager.PlayerCount, message, availableItems.Count, PlayerManager.PlayerCount);
		if (availableItems.Count <= PlayerManager.PlayerCount) // if assertion fails
		{
			EditorWindow.GetWindow<SceneView>().ShowNotification(new ("Not enough items available for distribution."), 5f);
			Debug.Break();
			return;
		}
#endif

		const int multiplier = 2;
		int minimumItems = Mathf.Max(2, PlayerManager.PlayerCount);
		int itemsToSpawn = Random.Range(minimumItems, (PlayerManager.PlayerCount * multiplier) + 1);
		
		// Shuffle the available items list
		availableItems = availableItems.OrderBy(_ => Random.value).ToList();
		
		var instantiatedObjects = new List<SceneItem>();
		for (int i = 0; i < itemsToSpawn && i < availableItems.Count; i++)
		{
			SceneItem instantiatedItem = Instantiate(availableItems[i], transform);
			instantiatedItem.gameObject.SetActive(true);
			instantiatedItem.gameObject.name = availableItems[i].name;
			instantiatedObjects.Add(instantiatedItem);
		}

		foreach (Player player in PlayerManager.Instance.Players)
		{
			player.InputManager.ToggleInputLayer("UI");
			SceneItem randomItem = instantiatedObjects[Random.Range(0, instantiatedObjects.Count)];
			player.InputManager.EventSystem.SetSelectedGameObject(randomItem.gameObject);
		}
		
		// TODO: Add a UI element to show the item selection process
		
		Debug.Log($"Item distribution initialized with {instantiatedObjects.Count} items.");
	}
	
	void DistributionComplete(Dictionary<SceneItem, Player> _)
	{
		foreach (Player player in results.Values)
		{
			player.InputManager.EventSystem.SetSelectedGameObject(null);
			player.InputManager.ToggleInputLayer("Player");
		}
		
		// TODO: Hide UI
		
		Debug.Log("All items have been distributed." + "\nResults: " + string.Join(", ", results.Select(kvp => $"{kvp.Key.name} -> {kvp.Value.name}")));
		
		Cleanup();
		
		return;
		void Cleanup()
		{
			playerVotes.Clear();
			finishedPlayers.Clear();
			results.Clear();
			
			// Destroy any remaining items in the scene
			SceneItem[] items = FindObjectsByType<SceneItem>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
			foreach (SceneItem item in items.Where(i => i != null)) Destroy(item.gameObject);
		}
	}

	public void Vote(Player player, SceneItem item)
	{
		// Don't accept votes from players who already have items
		if (finishedPlayers.Contains(player)) return;

		// Record the player's vote
		playerVotes[player] = item;
		OnItemVoted?.Invoke(item, player);

		// If there's only one player total, resolve immediately
		if (PlayerManager.PlayerCount == 1)
		{
			ResolveVotes();
			return;
		}

		// Check if all remaining active players have voted
		int remainingPlayers = PlayerManager.PlayerCount - finishedPlayers.Count;
		if (playerVotes.Count == remainingPlayers) ResolveVotes();
	}

	void ResolveVotes()
	{
		// Group votes by item
		IEnumerable<IGrouping<SceneItem, KeyValuePair<Player, SceneItem>>> groupedVotes = playerVotes.GroupBy(v => v.Value);

		foreach (IGrouping<SceneItem, KeyValuePair<Player, SceneItem>> group in groupedVotes)
		{
			if (group.Count() > 1) // If multiple players voted for the same item
			{
				// Randomly select one player to receive the item
				Player randomPlayer = group.Select(v => v.Key).OrderBy(_ => Random.value).First();

				AssignItemToPlayer(randomPlayer, group.Key);
				OnItemDistributed?.Invoke(group.Key, randomPlayer);
				Debug.Log($"[Selection] {randomPlayer.name} received {group.Key.name}.");

				FinishPlayerTurn(randomPlayer, group.Key);

				// Redirect other players who voted for this item to new selections
				RedirectOtherPlayers(group, randomPlayer);
			}
			else // If only one player voted for the item
			{
				Player player = group.First().Key;
				AssignItemToPlayer(player, group.Key);
				OnItemDistributed?.Invoke(group.Key, player);
				Debug.Log($"[Selection] {player} picked {group.Key.name}.");

				FinishPlayerTurn(player, group.Key);
			}
		}

		// Clear votes for the next round
		playerVotes.Clear();

		// Check if all players have received items
		if (finishedPlayers.Count < PlayerManager.PlayerCount)
		{
			// There is no need to set up the next round if the game is played single-player
			if (PlayerManager.PlayerCount == 1) return;

			// Continue the process for remaining players
			SetupNextRound();
		}
		else
		{
			// All players have received items, process is complete
			Debug.Log("Item selection process complete - all players have received items.");
		}
	}

	void FinishPlayerTurn(Player player, SceneItem item)
	{
		finishedPlayers.Add(player);
		if (finishedPlayers.Count == PlayerManager.PlayerCount)
		{
			// All players have finished their turns
			Debug.Log("[Selection] All players have finished their turns.");
			
			finishedPlayers.Clear();
			OnAllItemsDistributed?.Invoke(results);
		}
		
		player.InputManager.EventSystem.SetSelectedGameObject(null);

		// Disable the selected item
		item.gameObject.SetActive(false);
		item.gameObject.name += " (Picked)";
	}

	void RedirectOtherPlayers(IGrouping<SceneItem, KeyValuePair<Player, SceneItem>> group, Player winningPlayer)
	{
		foreach (KeyValuePair<Player, SceneItem> vote in group)
		{
			Player player = vote.Key;

			if (player != winningPlayer && !finishedPlayers.Contains(player))
			{
				SceneItem[] availableItems = FindObjectsByType<SceneItem>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

				if (availableItems.Length > 0)
				{
					SceneItem randomItem = availableItems[Random.Range(0, availableItems.Length)];
					player.InputManager.EventSystem.SetSelectedGameObject(randomItem.gameObject);
				}
			}
		}
	}

	void SetupNextRound()
	{
		// Get remaining active players
		IEnumerable<Player> remainingPlayers = PlayerManager.Instance.Players.Where(p => !finishedPlayers.Contains(p));

		foreach (Player player in remainingPlayers)
		{
			SceneItem[] availableItems = FindObjectsByType<SceneItem>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

			if (availableItems.Length > 0)
			{
				SceneItem randomItem = availableItems[Random.Range(0, availableItems.Length)];
				player.InputManager.EventSystem.SetSelectedGameObject(randomItem.gameObject);
			}
		}
	}

	void AssignItemToPlayer(Player player, SceneItem sceneItem)
	{
		player.Inventory.AddToInventory(sceneItem); 
		results[sceneItem] = player;
	}
}