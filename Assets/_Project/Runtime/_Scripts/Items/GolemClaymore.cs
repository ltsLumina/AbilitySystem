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

	// public void ReducePlayerMovementSpeed()
	// {
	//     var player = Helpers.Find<Player>();
	//     float originalSpeed = player.MovementSpeed;
	//     player.MovementSpeed *= 0.75f; // Reduce speed by 25%
	//
	//     StartCoroutine(RestoreMovementSpeed(player, originalSpeed, 5f));
	// }
	//
	// IEnumerator RestoreMovementSpeed(Player player, float originalSpeed, float delay)
	// {
	//     yield return new WaitForSeconds(delay);
	//     player.MovementSpeed = originalSpeed;
	// }
}
