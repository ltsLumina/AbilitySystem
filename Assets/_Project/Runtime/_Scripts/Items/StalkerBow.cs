using UnityEngine;

[CreateAssetMenu(fileName = "Stalker's Bow", menuName = "Items/Stalker's Bow")]
public class StalkerBow : Item
{
	public override void Action(Player owner)
	{
		owner.Stats.Add(Stats.StatType.Damage, 0.2f);
		Consumed = true;
	}
}
