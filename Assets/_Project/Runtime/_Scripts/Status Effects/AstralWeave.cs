public class AstralWeave : Buff
{
	public override void Reset()
	{
		statusName = "Astral Weave";
		description = "Allows Fire IV to be cast instantly, reduces spell speed by 25%, and increases damage by 10% for the full duration.";
		duration = 8;
		target = Target.Self;
		timing = Timing.Postfix;
	}

	protected override void OnInvoke()
	{
		OnInvoked += Invoked;
		OnDecayed += Decayed;
	}

	void Invoked(StatusEffect obj)
	{
		casterAsPlayer.Stats.Remove("cast speed", 1);
		casterAsPlayer.Stats.Remove("spell speed", 0.25f);
		casterAsPlayer.Stats.Add("damage", 0.1f);
	}

	void Decayed(StatusEffect obj)
	{
		casterAsPlayer.Stats.Add("cast speed", 1f);
		casterAsPlayer.Stats.Add("spell speed", 0.25f);
		casterAsPlayer.Stats.Remove("damage", 0.1f);
	}
}
