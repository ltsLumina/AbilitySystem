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
		
	}

	protected override void OnDecay()
	{
		if (!entity.TryGetComponent(out Boss boss)) return;
		float compiledDamage = boss.MaxHealth - boss.Health;

		const float threshold = 0.05f;

		if (compiledDamage >= boss.MaxHealth * threshold) // 5% of max health
		{
			foreach (Player player in PlayerManager.Instance.Players)
			{
				player.Stats.Add(Stats.StatType.Shields, 1);
				Debug.Log("Chain Stratagem shield granted to " + player.name);
			}
		}
		else { Debug.Log($"Chain Stratagem failed. \nCompiled damage: {compiledDamage} ({compiledDamage / initialHealth * 100}%)"); }
	}
}
