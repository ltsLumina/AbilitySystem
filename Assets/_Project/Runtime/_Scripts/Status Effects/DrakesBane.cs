public class DrakesBane : StatusEffect.Debuff
{
	protected override void Reset()
	{
		statusName = "Drakes Bane";
		description = "Lowers target physical damage dealt by 10%.";
		duration = 12;
		target = Target.Enemy;
		timing = Timing.Postfix;
	}
}
