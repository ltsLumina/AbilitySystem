
using UnityEngine;

[CreateAssetMenu(fileName = "New Status Effect", menuName = "Status Effects/Status Effect")]
public class DamageUp : StatusEffect.Effect.Buff
{
    public DamageUp(string statusName, int duration, string description, TargetType targets, Entity owner) : base(statusName, duration, description, targets, owner)
    {
    }
    
    public override void ApplyEffect(Entity target)
    {
        base.ApplyEffect(target);
        Debug.Log($"{target.name} took 10 damage.");
    }
}
