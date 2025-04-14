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

	protected override void OnInvoke()
	{
		OnInvoked += OnOnInvoked;
		OnDecayed += OnOnDecayed;
	}

	void OnOnInvoked(StatusEffect obj)
	{
		
	}

	void OnOnDecayed(StatusEffect obj)
	{
		
	}
}
