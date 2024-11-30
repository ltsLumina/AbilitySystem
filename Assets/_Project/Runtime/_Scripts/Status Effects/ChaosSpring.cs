#region
using UnityEngine;
#endregion

public class ChaosSpring : Debuff
{
	[SerializeField] float damage = 65;

	public override void Reset()
	{
		statusName = "Chaos Spring";
		description = "Deals unaspected damage over time.";
		duration = 30;
		target = Target.Enemy;
		timing = Timing.Postfix;
	}

	protected override void OnInvoke() => DamageOverTime(entity, damage);
}
