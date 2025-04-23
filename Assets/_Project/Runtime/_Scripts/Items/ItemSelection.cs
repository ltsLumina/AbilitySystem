#region
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using VInspector;
using Random = UnityEngine.Random;
#endregion

public class ItemSelection : MonoBehaviour
{
	[SerializeField] SerializedDictionary<Player, SceneItem> playerVotes = new ();

	public void Vote(Player player, SceneItem item)
	{
		// Record the player's vote
		playerVotes[player] = item;

		// Check if all players have voted
		if (playerVotes.Count == PlayerManager.Instance.Players.Count) ResolveVotes();
		
		Debug.Log($"{player} voted");
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

				// set the selected item for the other players to a random item that is not the item that was picked
				foreach (Player player in PlayerManager.Instance.Players)
				{
					if (player == randomPlayer) continue;

					SceneItem[] sceneItems = FindObjectsByType<SceneItem>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
					var randomItem = sceneItems[Random.Range(0, sceneItems.Length)];
					player.InputManager.GetComponent<MultiplayerEventSystem>().SetSelectedGameObject(randomItem.gameObject);
				}
				
				Debug.Log($"{randomPlayer.name} received {group.Key.name}.");
			}
			else // If only one player voted for the item
			{
				AssignItemToPlayer(group.First().Key, group.Key);

				var player = group.First().Key;
				player.InputManager.GetComponent<MultiplayerEventSystem>().SetSelectedGameObject(null);
			}

			SceneItem sceneItem = group.Key;
			sceneItem.gameObject.SetActive(false);
			sceneItem.name += " (Picked)";
		}

		// Clear votes for the next round
		playerVotes.Clear();
	}

	void AssignItemToPlayer(Player player, SceneItem sceneItem)
	{
		// Logic to assign the item to the player
		player.Inventory.AddToInventory(sceneItem);
	}
}