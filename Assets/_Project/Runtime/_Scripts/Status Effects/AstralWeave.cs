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
		casterAsPlayer.Attributes.Remove(Attributes.Stats.CastSpeed, 1);
		casterAsPlayer.Attributes.Remove(Attributes.Stats.SpellSpeed, 0.25f);
	}

	void Decayed(StatusEffect obj)
	{
		casterAsPlayer.Attributes.Add(Attributes.Stats.CastSpeed, 1f);
		casterAsPlayer.Attributes.Add(Attributes.Stats.SpellSpeed, 0.25f);
	}
}
