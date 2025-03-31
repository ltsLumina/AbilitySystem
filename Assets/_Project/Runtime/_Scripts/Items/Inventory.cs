#region
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using VInspector;
#endregion

public class Inventory : MonoBehaviour
{
	[SerializeField] List<Item> inventory = new ();

	readonly Dictionary<Item, float> cooldowns = new (); // the current cooldown time of each item

	[Button] [UsedImplicitly]
	public void AddGolemClaymore()
	{
		var item = Resources.Load<Item>("Scriptables/Items/Golem's Claymore");
		AddToInventory(item);
	}

	[Button] [UsedImplicitly]
	public void AddPhoenixCharm()
	{
		var item = Resources.Load<Item>("Scriptables/Items/Phoenix Charm");
		AddToInventory(item);
	}

	void AddToInventory(Item item)
	{
		if (item == null) return;

		inventory.Add(item);
		cooldowns.Add(item, item.Cooldown);
	}

	void Start()
	{
		foreach (Item item in inventory) cooldowns.Add(item, item.Cooldown);

		//Debug.Log($"{item.name} has an initial cooldown of {item.Cooldown}.");
	}

	void Update()
	{
		foreach (Item item in inventory)
		{
			if (item == null) continue;

			// if the cooldown is greater than 0, decrement it
			if (cooldowns[item] > 0)
			{
				cooldowns[item] -= Time.deltaTime;

				//Debug.Log($"{item.name} has a current cooldown of {cooldowns[item]}.");
			}

			// if the cooldown is zero, invoke the item's action and reset the cooldown
			if (cooldowns[item] <= 0)
			{
				item.Action();
				Debug.Log($"{item.name} has been invoked.");
				cooldowns[item] = item.Cooldown;
			}
		}
	}
}
