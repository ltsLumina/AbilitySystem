#region
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using VInspector;
#endregion

public class Inventory : MonoBehaviour
{
	[SerializeField] List<Item> inventory = new ();

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
		var player = GetComponent<Player>();
		int id = player.ID;

		int x = id switch
		{ 1 => 175,
		  2 => 500,
		  3 => 830,
		  _ => 0 };

		var rect = new Rect(x, 565, 400, 100);
		
		GUILayout.BeginArea(rect);

		foreach (Item item in inventory)
		{
			if (item == null) continue;
			
			if (cooldowns[item] <= 0) GUILayout.Label($"{item.name} - Passive");
			else GUILayout.Label($"{item.name} - {cooldowns[item].RoundTo(2)}");
		}

		GUILayout.EndArea();
	}
#endif

	public void AddToInventory([NotNull] Item item)
	{
		if (item == null) return;
		if (HasItem(item, true)) return;
		
		// create instance of item
		Item instance = Instantiate(item);
		instance.name = item.name;
		instance.name += '*'; // add asterisk to the name to indicate it's an instance
		
		inventory.Add(instance);
		cooldowns.Add(instance, instance.Cooldown);
		//Logger.Log($"{instance.name} has been added to the inventory.", null, "Inventory");

		if (item.InvokeWhenAdded)
		{
			var owner = GetComponent<Player>();
			instance.Action(owner);
		}
	}

	public void AddToInventory([NotNull] SceneItem item) => AddToInventory(item.RepresentedItem);

	public bool HasItem([NotNull] Item item, bool log = false)
	{
		if (inventory.Any(i => i.Name == item.name)) // Note: Can't check references directly because each item is an instance. Therefore we check the name.
		{
			if (log) Logger.Log($"{item.name} is in the inventory.", item, "Inventory");
			return true;
		}

		if (log) Logger.Log($"{item.name} is not in the inventory.", item, "Inventory");
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
