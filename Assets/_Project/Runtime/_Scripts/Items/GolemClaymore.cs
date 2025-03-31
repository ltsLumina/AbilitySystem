#region
using Lumina.Essentials.Modules;
using UnityEngine;
#endregion

[CreateAssetMenu(fileName = "New Golem's Claymore", menuName = "Items/Golem's Claymore")]
public class GolemClaymore : Item
{
	public override void Action()
	{
		Debug.Log("Golem Claymore invoked.");

		var player = Helpers.Find<Player>();
		var quickSpell = StatusEffect.CreateCustomStatusEffect("Golem's Might", "Movement speed is reduced.", 5, StatusEffect.Target.Self, StatusEffect.Timing.Prefix);
		player.AddStatusEffect(quickSpell);

		GameObject enemy = GameObject.Find("Enemy");
		enemy.GetComponent<Dummy>().TakeDamage(damage); // temp
	}
}
