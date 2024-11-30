public class ChaosSpring : StatusEffect.Debuff
{
	protected override void Reset()
	{
		statusName = "Chaos Spring";
		description = "Deals unaspected damage over time.";
		duration = 30;
		target = Target.Enemy;
		timing = Timing.Postfix;
	}
}
