public class Phlegmacism : Buff
{
    public override void Reset()
    {
        statusName = "Phlegmacism";
        description = "Cast a divine veil around yourself and your allies, protecting everyone from one instance of damage.";
        duration = 12;
        target = Target.Self | Target.Ally;
        timing = Timing.Prefix;
    }

    protected override void OnInvoke()
    {
        entity.TryGetComponent(out Player player);

        if (!player)
        {
            Logger.LogError("how");
            return;
        }

        player.Attributes.Add(Attributes.Stats.Shields, 1);

        player.OnTookDamage += RemoveEffectOnDamageTaken;
    }

    void RemoveEffectOnDamageTaken(bool hadShields)
    {
        if (entity.HasStatusEffect(this, out _)) entity.RemoveStatusEffect(this);
    }

    protected override void OnDecay()
    {
        if (entity.HasStatusEffect(this, out _))
        {
            entity.TryGetComponent(out Player player);

            if (!player)
            {
                Logger.LogError("how");
                return;
            }

            player.Attributes.Remove(Attributes.Stats.Shields, 1);

            //player.OnTookDamage -= RemoveEffectOnDamageTaken;
        }
    }
}