#region
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using VInspector;
#endregion

public class Inventory : MonoBehaviour
{
	[SerializeField] List<Item> inventory = new ();
	[Space(10)]
	[Header("Debug")]

	[SerializeField] bool showGUI;

	readonly Dictionary<Item, float> cooldowns = new (); // the current cooldown time of each item

#if UNITY_EDITOR
	[SerializeField] Item itemToAdd;

	[Button] [UsedImplicitly]
	public void AddItem()
	{
		AddToInventory(itemToAdd);
	}
#endif

	void Start()
	{
		foreach (Item item in inventory) cooldowns.Add(item, item.Cooldown);

		//Debug.Log($"{item.name} has an initial cooldown of {item.Cooldown}.");

		GameManager.Instance.OnEnterBattle += () =>
		{ // reset the cooldowns when entering battle
			foreach (Item item in inventory)
			{
				if (item == null) continue;
				if (item.Consumed) continue;

				cooldowns[item] = item.Cooldown;
			}
		};
	}

	void Update() => TickItems();

#if UNITY_EDITOR
	void OnGUI()
	{
		if (!showGUI) return;

		GUILayout.BeginArea(new (Screen.width - 200, 10, 190, Screen.height - 20));

		foreach (Item item in inventory)
		{
			if (item == null) continue;

			if (cooldowns[item] <= 0) GUILayout.Label($"{item.name} - Passive");
			else GUILayout.Label($"{item.name} - {cooldowns[item].RoundTo(2)}");
		}

		GUILayout.EndArea();
	}
#endif

	public void AddToInventory(Item item)
	{
		if (item == null) return;

		if (inventory.Contains(item))
		{
			Debug.LogWarning($"[Inventory] {item.name} is already in the inventory.");
			return;
		}

		inventory.Add(item);
		cooldowns.Add(item, item.Cooldown);
		Debug.Log($"[Inventory] {item.name} has been added to the inventory.");

		if (item.InvokeWhenAdded)
		{
			var owner = GetComponent<Player>();
			item.Action(owner);
		}
	}

	public void AddToInventory(SceneItem item) => AddToInventory(item.RepresentedItem);

	public bool HasItem([NotNull] Item item, bool log = false)
	{
		if (inventory.Contains(item))
		{
			if (log) Debug.Log($"[Inventory] {item.name} is in the inventory.");
			return true;
		}

		if (log) Debug.LogWarning($"[Inventory] {item.name} is not in the inventory.");
		return false;
	}
	
	public bool HasItem([NotNull] SceneItem item) => HasItem(item.RepresentedItem);
	
	void TickItems()
	{
		if (GameManager.Instance.CurrentState != GameManager.State.Battle) return;

		foreach (Item item in inventory)
		{
			if (item == null) continue;
			if (item.Consumed) continue;

			// if the cooldown is greater than 0, decrement it
			if (cooldowns[item] > 0)
			{
				cooldowns[item] -= Time.deltaTime;

				//Debug.Log($"{item.name} has a current cooldown of {cooldowns[item]}.");
			}

			// if the cooldown is zero, invoke the item's action and reset the cooldown
			if (cooldowns[item] <= 0)
			{
				item.Action(GetComponent<Player>());

				//Debug.Log($"[Inventory] {item.name} has been invoked.");
				cooldowns[item] = item.Cooldown;
			}
		}
	}
}

public static class MathExtensions
{
	public static float RoundTo(this float value, int digits)
	{
		float multiplier = Mathf.Pow(10, digits);
		return Mathf.Round(value * multiplier) / multiplier;
	}
}
