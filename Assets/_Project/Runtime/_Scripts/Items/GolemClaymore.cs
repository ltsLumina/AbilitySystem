#region
using UnityEngine;
#endregion

[CreateAssetMenu(fileName = "New Golem's Claymore", menuName = "Items/Golem's Claymore")]
public class GolemClaymore : Item
{
	public override void Action(Player owner)
	{
		// TODO: temporary debuff
		owner.Stats.Remove(Stats.StatType.Speed, 0.10f);

		Boss enemy = GameManager.Instance.CurrentBoss;
		enemy.TakeDamage(damage);
	}
}
