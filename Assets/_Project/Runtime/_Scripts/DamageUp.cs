using UnityEngine;

[CreateAssetMenu]
public class DamageUp : StatusEffect.Buff
{
    public override void ApplyEffect(Entity target)
    {
        base.ApplyEffect(target);
        Debug.Log($"Applied {name} to {target}.");
    }
}
