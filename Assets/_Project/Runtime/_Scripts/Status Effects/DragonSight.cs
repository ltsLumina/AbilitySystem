public class DragonSight : StatusEffect.Buff
{
	protected override void Reset()
	{
		statusName = "Dragon Sight";
		description = "Grants Dragon Sight to self, increasing damage dealt by 10%";
		duration = 24;
		target = Target.Self;
		timing = Timing.Prefix;
	}

	public override void Invoke(Entity entityTarget)
	{
		base.Invoke(entityTarget);
		Ability.damageMod = 1.5f;
	}
}
