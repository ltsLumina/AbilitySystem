public class Dualcast : Buff
{
	public override void Reset()
	{
		statusName = "Dualcast";
		description = "The next spell is cast twice instantly.";
		duration = 8;
		target = Target.Self;
		timing = Timing.Postfix;
	}
}
