#region
using Lumina.Essentials.Modules;
using UnityEngine;
#endregion

[CreateAssetMenu(fileName = "New Golem's Claymore", menuName = "Items/Golem's Claymore")]
public class GolemClaymore : Item
{
	public override void Action()
	{
		var player = Helpers.Find<Player>();
		var quickSpell = StatusEffect.CreateCustomStatusEffect("Golem's Might", "Movement speed is reduced.", 5, StatusEffect.Target.Self, StatusEffect.Timing.Prefix);

		quickSpell.OnInvoked += _ => player.Stats.Add("speed", -0.15f);
		quickSpell.OnDecayed += _ => player.Stats.Add("speed", 0.15f);

		var enemy = FindFirstObjectByType<Boss>();
		enemy.TakeDamage(damage);
	}
}
