#region
using UnityEngine;
#endregion

[CreateAssetMenu(fileName = "New Golem's Claymore", menuName = "Items/Golem's Claymore")]
public class GolemClaymore : Item
{
	public override void Action(Player owner)
	{
		var quickSpell = StatusEffect.CreateCustomStatusEffect("Golem's Might", "Movement speed is reduced.", 5, StatusEffect.Target.Self, StatusEffect.Timing.Prefix, owner);

		quickSpell.OnInvoked += _ => owner.Stats.Remove("speed", 0.15f);
		quickSpell.OnDecayed += _ => owner.Stats.Add("speed", 0.15f);
		quickSpell.Invoke(owner);

		Boss enemy = GameManager.Instance.CurrentBoss;
		enemy.TakeDamage(damage);
	}
}
