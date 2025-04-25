public class Seraphism : Buff
{
	public override void Reset()
	{
		statusName = "Seraphism";
		description = "Reduces cast time by 20%, spell speed by 15%, and increases damage dealt by 10% for self and allies.";
		duration = 12;
		target = Target.Self | Target.Ally;
		timing = Timing.Postfix;
	}

	protected override void OnInvoke()
	{
		entity.TryGetComponent(out Player player);
		player.Stats.Remove(Stats.StatType.CastSpeed, 0.2f);
		player.Stats.Remove(Stats.StatType.SpellSpeed, 0.15f);
		player.Stats.Add(Stats.StatType.Damage, 0.1f);
	}

	protected override void OnDecay()
	{
		entity.TryGetComponent(out Player player);
		player.Stats.Add(Stats.StatType.CastSpeed, 0.2f);
		player.Stats.Add(Stats.StatType.SpellSpeed, 0.15f);
		player.Stats.Remove(Stats.StatType.Damage, 0.1f);
	}
}
