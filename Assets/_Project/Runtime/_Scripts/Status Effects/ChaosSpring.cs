#region
using UnityEngine;
#endregion

public class ChaosSpring : Debuff
{
	[SerializeField] float damage = 65;

	public override void Reset()
	{
		statusName = "Chaos Spring";
		description = "Deals unaspected damage over time.";
		duration = 30;
		target = Target.Enemy;
		timing = Timing.Postfix;
	}

	protected override void OnInvoke()
	{
		if (entity.gameObject.TryGetComponent(out DamageOverTime existingDoT)) { Debug.LogWarning("[ChaosSpring] Damage over time already exists."); }
		else
		{
			var damageOverTime = entity.gameObject.AddComponent<DamageOverTime>();
			caster.TryGetComponent(out Player player);
			damageOverTime.Apply(entity, duration, damage, player);
		}
	}
}

public class DamageOverTime : MonoBehaviour
{
	public void Apply(Entity entityTarget, int duration, float damage, Player player)
	{
		int cycle = 0;
		int dotTick = 0;
		int dotTicks = duration / AbilitySettings.DoT_Rate - 1;

		// Reset the cycle count if the DoT is already running
		TickManager.OnCycle -= OnCycle;
		TickManager.OnCycle += OnCycle;

		return;

		void OnCycle()
		{
			if (dotTick == dotTicks)
			{
				// DoT has expired
				TickManager.OnCycle -= OnCycle;

				Destroy(this);
				return;
			}

			cycle++;

			if (cycle % AbilitySettings.DoT_Rate == 0) // If DoT_Rate is 3, this will tick on cycle 3, 6, 9, etc.
			{
				if (entityTarget != null)
				{
					if (entityTarget.TryGetComponent(out IDamageable damageable)) damageable?.TakeDamage(damage * player.Stats.Damage);
				}
				
				dotTick++;
			}
		}
	}
}
