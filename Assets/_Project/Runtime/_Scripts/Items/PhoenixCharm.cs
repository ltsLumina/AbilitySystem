#region
using Lumina.Essentials.Modules;
using UnityEngine;
#endregion

[CreateAssetMenu(fileName = "Phoenix Charm", menuName = "Items/Phoenix Charm")]
public class PhoenixCharm : Item
{
	public override void Action()
	{
		Debug.Log("Phoenix Charm invoked.");

		var player = Helpers.Find<Player>();
		player.Stats.Add("shields", 1);

		Consumed = true;
	}
}
