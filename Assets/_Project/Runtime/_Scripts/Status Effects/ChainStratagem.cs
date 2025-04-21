#region
using UnityEngine;
#endregion

public class ChainStratagem : Debuff
{
	public override void Reset()
	{
		statusName = "Chain Stratagem";
		description = "Compiles damage dealt to target for 10 seconds. If damage exceeds 5% of target's Max Health, grants a shield to self and contributing allies.";
		duration = 10;
		target = Target.Enemy;
		timing = Timing.Prefix;
	}

	float initialHealth;

	protected override void OnInvoke()
	{
		entity.TryGetComponent(out Boss boss);
		if (!boss) return;

		initialHealth = boss.Health;
	}

	protected override void OnDecay()
	{
		if (!entity.TryGetComponent(out Boss boss)) return;
		float compiledDamage = initialHealth - boss.Health;

		if (compiledDamage >= initialHealth * 0.05f)
		{
			foreach (Player player in PlayerManager.Instance.Players)
			{
				player.Stats.Add("shields", 1);
				Debug.Log("Chain Stratagem shield granted to " + player.name);
			}
		}
	}
}
