#region
#endregion

public class DragonSight : Buff
{
	public override void Reset()
	{
		statusName = "Dragon Sight";
		description = "Grants Dragon Sight to self, increasing damage dealt by 10%";
		duration = 24;
		target = Target.Self;
		timing = Timing.Prefix;
	}

	protected override void OnInvoke()
	{
		caster.TryGetComponent(out Player player);
		player.Attributes.Add(Attributes.Stats.Damage, 0.1f);
	}

	protected override void OnDecay()
	{
		caster.TryGetComponent(out Player player);
		player.Attributes.Remove(Attributes.Stats.Damage, 0.1f);
	}
}
