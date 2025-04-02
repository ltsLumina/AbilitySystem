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

	protected override void OnInvoke()
	{
		Modify();

		player.Modifiers.Add("Damage", 0.1f);
	}

	protected override void OnDecay()
	{
		Modify(true);

		player.Modifiers.Remove("Damage", 0.1f);
	}

	void Modify(bool remove = false) => Debug.Log($"{entity.name} has {(remove ? "lost" : "gained")} Dragon Sight.");
}
