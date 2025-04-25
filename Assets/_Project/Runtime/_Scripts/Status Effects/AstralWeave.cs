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
		casterAsPlayer.Stats.Remove(Stats.StatType.CastSpeed, 1);
		casterAsPlayer.Stats.Remove(Stats.StatType.SpellSpeed, 0.25f);
		casterAsPlayer.Stats.Add(Stats.StatType.Damage, 0.1f);
	}

	protected override void OnDecay()
	{
		casterAsPlayer.Stats.Add(Stats.StatType.CastSpeed, 1f);
		casterAsPlayer.Stats.Add(Stats.StatType.SpellSpeed, 0.25f);
		casterAsPlayer.Stats.Remove(Stats.StatType.Damage, 0.1f);
	}
}
