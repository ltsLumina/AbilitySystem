#region
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
			}
			else // If only one player voted for the item
				AssignItemToPlayer(group.First().Key, group.Key);
		}

		// Clear votes for the next round
		playerVotes.Clear();
	}

	void AssignItemToPlayer(Player player, SceneItem item)
	{
		Debug.Log($"Assigned {item.name} to {player.name}");

		// Logic to assign the item to the player
		
	}
}