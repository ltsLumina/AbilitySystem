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
		player.Attributes.Remove(Attributes.Stats.CastSpeed, 0.2f);
		player.Attributes.Remove(Attributes.Stats.SpellSpeed, 0.15f);
		player.Attributes.Add(Attributes.Stats.Damage, 0.1f);
	}

	protected override void OnDecay()
	{
		entity.TryGetComponent(out Player player);
		player.Attributes.Add(Attributes.Stats.CastSpeed, 0.2f);
		player.Attributes.Add(Attributes.Stats.SpellSpeed, 0.15f);
		player.Attributes.Remove(Attributes.Stats.Damage, 0.1f);
	}
}
