public class Phlegmaticism : Buff
{
	public override void Reset()
	{
		statusName = "Phlegmaticism";
		description = "Cast a divine veil around yourself and your allies, protecting everyone from one instance of damage.";
		duration = 12;
		target = Target.Self | Target.Ally;
		timing = Timing.Prefix;
	}
}
