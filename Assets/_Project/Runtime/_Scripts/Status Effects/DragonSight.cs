#region
using UnityEngine;
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

	protected override void OnInvoke() => Modify();

	protected override void OnDecay() => Modify(true);

	void Modify(bool remove = false)
	{
		Modifiers modifiers = entity.Modifiers;
		modifiers.DamageMod = remove ? 1f : 1.1f;
		entity.Modifiers = modifiers;

		Debug.Log($"{entity.name} has {(remove ? "lost" : "gained")} Dragon Sight.");
	}
}
