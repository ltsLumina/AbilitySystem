#region
using System.Linq;
#endregion

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
        player?.Stats.Add("shields", 1);
    }

    protected override void OnDecay()
    {
        foreach (Player player in PlayerManager.Instance.Players.Where(p => p.HasStatusEffect(this, out _))) player.Stats.Remove("shields", 1);
    }
}