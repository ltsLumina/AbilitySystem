#region
using UnityEngine;
#endregion

[CreateAssetMenu(fileName = "Phoenix Charm", menuName = "Items/Phoenix Charm")]
public class PhoenixCharm : Item
{
	public override void Action(Player owner)
	{
		owner.OnHit += () =>
		{
			if (owner.Health == 1 && owner.Stats.Shields == 0)
			{
				owner.Stats.Add(Stats.StatType.Shields, 1);
				owner.Heal(owner.MaxHealth);
			}
		};

		Consumed = true;
	}
}
