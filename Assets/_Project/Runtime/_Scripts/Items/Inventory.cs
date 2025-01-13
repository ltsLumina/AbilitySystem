#region
using System;
using System.Collections.Generic;
using UnityEngine;
#endregion

public class Inventory : MonoBehaviour
{
	[SerializeField] List<Item> inventory = new ();

	readonly Dictionary<Item, float> cooldowns = new (); // the current cooldown time of each item

	public Action OnItemAdded;

	void OnEnable() => OnItemAdded += Start;

	void OnDisable() => OnItemAdded -= Start;

	void Start()
	{
		foreach (Item item in inventory) cooldowns.Add(item, item.Cooldown);

		//Debug.Log($"{item.name} has an initial cooldown of {item.Cooldown}.");
	}

	void Update()
	{
		foreach (Item item in inventory)
		{
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
