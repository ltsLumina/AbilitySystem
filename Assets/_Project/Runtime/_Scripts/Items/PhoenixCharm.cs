#region
using UnityEngine;
#endregion

[CreateAssetMenu(fileName = "Phoenix Charm", menuName = "Items/Phoenix Charm")]
public class PhoenixCharm : Item
{
	public override void Action(Player owner)
	{
		owner.Attributes.Add(Attributes.Stats.Shields, 1);

		Consumed = true;
	}
}
