#region
using UnityEngine;
#endregion

[CreateAssetMenu(fileName = "Phoenix Charm", menuName = "Items/Phoenix Charm")]
public class PhoenixCharm : Item
{
	public override void Action(Player owner)
	{
		owner.Stats.Add("shields", 1);

		Consumed = true;
	}
}
