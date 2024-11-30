#region
using UnityEngine;
#endregion

[CreateAssetMenu]
public class DamageUp : StatusEffect.Buff
{
	public override void ApplyEffect(Entity enemy)
	{
		base.ApplyEffect(enemy);
		Debug.Log($"Applied {name} to {enemy}.");
	}

	void Awake() => OnInstantiated += effect =>
	{
		effect.Time = effect.Duration;
		Debug.Log($"Instantiated: {effect}");
	};

	void Reset()
	{
		statusName = name;
		description = "Please enter a description.";
		duration = 30;
		target = TargetType.Self;
		appliesEarly = false;
	}
}
