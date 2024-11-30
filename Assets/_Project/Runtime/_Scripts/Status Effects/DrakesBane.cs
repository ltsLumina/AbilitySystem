public class DrakesBane : Debuff
{
	public override void Reset()
	{
		statusName = "Drakes Bane";
		description = "Lowers target physical damage dealt by 10%.";
		duration = 30;
		target = Target.Enemy;
		timing = Timing.Postfix;
	}
}
