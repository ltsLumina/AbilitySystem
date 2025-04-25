#define UNITY_ASSERTIONS

#region
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using JetBrains.Annotations;
using MelenitasDev.SoundsGood;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VInspector;
using Random = UnityEngine.Random;
#endregion

public class ItemDistributor : MonoBehaviour
{
	[SerializeField] SerializedDictionary<Player, SceneItem> playerVotes = new ();
	[SerializeField] List<Player> finishedPlayers = new ();

	Transform background;
	
	readonly Dictionary<SceneItem, Player> results = new ();

	public event Action<SceneItem, Player> OnItemVoted;
	public event Action<SceneItem, Player> OnItemDistributed;
	public event Action<Dictionary<SceneItem, Player>> OnAllItemsDistributed;

#if UNITY_EDITOR
	[Button] [UsedImplicitly]
	public void ResetUI() => transform.GetChild(0).localPosition = new (0, 1000, 0);
#endif

	void Awake()
	{
		background = transform.GetChild(0);
		
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
			SceneItem instantiatedItem = Instantiate(availableItems[i], background);
			instantiatedItem.gameObject.SetActive(false);
			instantiatedItem.gameObject.name = availableItems[i].name;
			instantiatedObjects.Add(instantiatedItem);
			
			// Add the 'Votes' child to the instantiated item
			var prefab = Resources.Load<GameObject>("PREFABS/UI/Votes");
			GameObject votes = Instantiate(prefab, instantiatedItem.transform);
			votes.transform.SetAsLastSibling();
		}

		foreach (Player player in PlayerManager.Instance.Players)
		{
			player.InputManager.ToggleInputLayer("UI");
		}
		
		// TODO: Add a UI element to show the item selection process
		ShowUI(instantiatedObjects);
		
		Logger.Log($"Item distribution initialized with {instantiatedObjects.Count} items.", this, "Distributor");
	}

	void ShowUI(List<SceneItem> instantiatedObjects)
	{
		var sequence = DOTween.Sequence();
		sequence.OnStart(() =>
		{
			background.localPosition = new (background.localPosition.x, 1000, background.localPosition.z);
			var color = background.GetComponent<Image>().color;
			color.a = 0.95f;
			background.GetComponent<Image>().color = color;
		});
		sequence.Append(background.DOLocalMoveY(0, 1f).SetEase(Ease.InOutSine));
		sequence.OnComplete(() =>
		{
			foreach (SceneItem instantiatedObject in instantiatedObjects)
			{
				var sequence = DOTween.Sequence();
				sequence.OnStart(() =>
				{
					instantiatedObject.gameObject.SetActive(true);
					Color color = instantiatedObject.Sprite.color;
					color.a = 0;
					instantiatedObject.Sprite.color = color;
				});
				sequence.Append(instantiatedObject.Sprite.DOFade(1, 0.5f).SetEase(Ease.InOutSine));
			}
		});

		foreach (Player player in PlayerManager.Instance.Players)
		{
			SceneItem randomItem = instantiatedObjects[Random.Range(0, instantiatedObjects.Count)];
			player.InputManager.EventSystem.SetSelectedGameObject(randomItem.gameObject);

			// spawn selection marker
			var prefab = Resources.Load<Selection>("PREFABS/UI/Selection");
			Selection selectionMarker = Instantiate(prefab, randomItem.transform);
			selectionMarker.Set(player);
		}
		
		GameManager.FadeOutBackground();
	}

	void HideUI()
	{
		var sequence = DOTween.Sequence();
		sequence.Append(background.GetComponent<Image>().DOFade(0, 0.5f).SetEase(Ease.InOutSine));
		sequence.OnComplete(() => background.localPosition = new (background.localPosition.x, 1000, background.localPosition.z));
		
		GameManager.FadeInBackground();
	}
	
	void DistributionComplete(Dictionary<SceneItem, Player> _)
	{
		foreach (Player player in results.Values)
		{
			player.InputManager.EventSystem.SetSelectedGameObject(null);
			player.InputManager.ToggleInputLayer("Player");
		}
		
		HideUI();

		StartCoroutine(PlayLevelUpSound());
		
		Logger.Log("All items have been distributed." + "\nResults: " + string.Join(", ", results.Select(kvp => $"{kvp.Key.name} -> {kvp.Value.name}")), null, "Distributor");
		
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

		IEnumerator PlayLevelUpSound()
		{
			float musicVolume = AudioManager.GetLastSavedOutputVolume(Output.Music);
			
			// Lower the music volume first
			AudioManager.ChangeOutputVolume(Output.Music, 0.1f);

			// Play the sound
			var sound = new Sound(SFX.LevelUp);
			sound.SetOutput(Output.SFX);
			sound.Play();

			// Wait for the sound to finish
			yield return new WaitForSeconds(6f);

			// Restore the original music volume
			AudioManager.ChangeOutputVolume(Output.Music, musicVolume);
		}
	}

	public void Vote(Player player, SceneItem item)
	{
		// Don't accept votes from players who already have items
		if (finishedPlayers.Contains(player)) return;

		// Record the player's vote
		playerVotes[player] = item;
		OnItemVoted?.Invoke(item, player);
		item.transform.GetChild(1).GetChild(player.PlayerInput.playerIndex).gameObject.GetComponent<Image>().color = player.AccentColour;

		// If there's only one player total, resolve immediately
		if (PlayerManager.PlayerCount == 1)
		{
			StartCoroutine(ResolveVotes());
			return;
		}

		// Check if all remaining active players have voted
		int remainingPlayers = PlayerManager.PlayerCount - finishedPlayers.Count;
		if (playerVotes.Count == remainingPlayers) StartCoroutine(ResolveVotes());
	}

	public void Unvote(Player player, SceneItem item)
	{
		playerVotes.Remove(player);
		
		item.transform.GetChild(1).GetChild(player.PlayerInput.playerIndex).gameObject.GetComponent<Image>().color = Color.clear;
	}

	IEnumerator ResolveVotes()
	{
		// Group votes by item
		IEnumerable<IGrouping<SceneItem, KeyValuePair<Player, SceneItem>>> groupedVotes = playerVotes.GroupBy(v => v.Value);

		foreach (IGrouping<SceneItem, KeyValuePair<Player, SceneItem>> group in groupedVotes)
		{
			if (group.Count() > 1) // If multiple players voted for the same item
			{
				yield return StartCoroutine(DistributeItemToRandomPlayer(group));
			}
			else // If only one player voted for the item
			{
				Player player = group.First().Key;
				AssignItemToPlayer(player, group.Key);
				OnItemDistributed?.Invoke(group.Key, player);

				FinishPlayerTurn(player, group.Key);
			}
		}

		// Clear votes for the next round
		playerVotes.Clear();

		// Check if all players have received items
		if (finishedPlayers.Count < PlayerManager.PlayerCount)
		{
			// There is no need to set up the next round if the game is played single-player
			if (PlayerManager.PlayerCount == 1) yield break;

			// Continue the process for remaining players
			SetupNextRound();
		}
		else
		{
			// All players have received items, process is complete
			Logger.Log("Item selection process complete - all players have received items.", this, "Distributor");
		}
	}

	readonly Dictionary<Player, int> rolls = new ();

	IEnumerator DistributeItemToRandomPlayer(IGrouping<SceneItem, KeyValuePair<Player, SceneItem>> group)
	{
		// All players in group roll a dice (1-100)
		
		foreach (KeyValuePair<Player, SceneItem> vote in group)
		{
			Player player = vote.Key;
			int roll = Random.Range(1, 101); // maxExclusive
			rolls.Add(player, roll);
			Logger.Log($"{player.name} rolled a {roll} for {group.Key.name}.", this, "Distributor");
		}

		// Find the highest roll value
		int highestRoll = rolls.Max(r => r.Value);

		// Get all players who rolled the highest value
		List<KeyValuePair<Player, int>> highestRollers = rolls.Where(r => r.Value == highestRoll).ToList();

		// If there's a tie for highest roll
		if (highestRollers.Count > 1)
		{
			Logger.Log($"Tie detected! {highestRollers.Count} players rolled {highestRoll}. Re-rolling between tied players...", this, "Distributor");

			// Clear the rolls dictionary and only re-roll for the tied players
			rolls.Clear();

			foreach (KeyValuePair<Player, int> tiedRoll in highestRollers)
			{
				Player player = tiedRoll.Key;
				int newRoll = Random.Range(1, 101);
				rolls.Add(player, newRoll);
				Logger.Log($"Tie-breaker: {player.name} rolled a {newRoll} for {group.Key.name}.", this, "Distributor");
			}

			yield return new WaitForSeconds(1f); // added suspense (re-roll delay)

			// Recursively handle the tie-breaker rolls (in case of another tie)
			yield return StartCoroutine(DistributeItemToRandomPlayer(group));
			yield break;
		}

		// If no tie, proceed with the highest roller
		Player winningPlayer = highestRollers.First().Key;

		yield return new WaitForSeconds(1.5f); // added suspense

		AssignItemToPlayer(winningPlayer, group.Key);
		OnItemDistributed?.Invoke(group.Key, winningPlayer);
		Logger.Log($"{winningPlayer.name} received {group.Key.name}.", this, "Distributor");

		FinishPlayerTurn(winningPlayer, group.Key);

		// Unvote the other players so they can re-select
		List<KeyValuePair<Player, SceneItem>> otherPlayers = group.Where(v => v.Key != winningPlayer).ToList();
		otherPlayers.ForEach(player => player.Key.PlayerInput.actions["Navigate"].Enable());

		// Redirect other players who voted for this item to new selections
		RedirectOtherPlayers(group, winningPlayer);
	}

	void FinishPlayerTurn(Player player, SceneItem item)
	{
		finishedPlayers.Add(player);
		if (finishedPlayers.Count == PlayerManager.PlayerCount)
		{
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