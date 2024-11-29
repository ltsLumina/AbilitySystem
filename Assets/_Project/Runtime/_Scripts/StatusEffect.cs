#region
using System.Collections.Generic;
using DG.Tweening;
using Lumina.Essentials.Attributes;
using Unity.Properties;
using UnityEngine;
using Object = UnityEngine.Object;
#endregion

public class StatusEffect : ScriptableObject
{
    public enum TargetType
    {
        Self,
        Enemy,
    }

    [SerializeField] string statusName;
    [SerializeField] int duration;
    [SerializeField] string description;
    [SerializeField] TargetType targets;
    [SerializeField] public bool appliesEarly;
    [Tooltip("The time remaining for the status effect.")]
    [SerializeField, ReadOnly] float time;
    [Tooltip("The entity that applied the status effect. (Not the entity that the status effect is applied to.)")]
    [SerializeField, ReadOnly] Entity caster;
    
    public string StatusName => statusName;
    public int Duration => duration;
    public float Time
    {
        get => time;
        set => time = value;
    }
    public Entity Caster => caster;

    public override string ToString() => $"{statusName ??= name} ({duration} seconds)";

    Player player => FindAnyObjectByType<Player>();
    
    public virtual void ApplyEffect(Entity target)
    {
        time = duration;
        caster = targets == TargetType.Self ? player : target;
        target = targets == TargetType.Self ? player : target;
        
        VisualEffect(target);
        target.ApplyStatusEffects(this);
    }

    #region Effects
    public class Buff : StatusEffect // Mostly a marker class
    {
        
    }

    public class Debuff : StatusEffect // Mostly a marker class
    {

    }

    public struct Buffs
    {
        
    }

    public struct Debuffs
    {

    }

    public struct All
    {

    }
    #endregion

    #region Utility
    static GameObject GetPooledObject(GameObject prefab) => ObjectPoolManager.FindObjectPool(prefab, 5).GetPooledObject(true);
    protected static void VisualEffect(Entity target, bool isDoT = false)
    {
        var prefab = Resources.Load<GameObject>("PREFABS/Effect");
        GameObject pooled = GetPooledObject(prefab);
        pooled.transform.position = target.transform.position;
        pooled.transform.localScale = isDoT ? new (0.5f, 0.5f) : new (1, 1);
        var sprite = pooled.GetComponent<SpriteRenderer>();

        sprite.DOFade(0, 1).OnComplete
        (() =>
        {
            sprite.DOFade(1, 0);
            pooled.SetActive(false);
        });
    }
    #endregion
}

public static class StatusEffectExtensions
{
    static StatusEffect Load(StatusEffect original)
    {
        try
        {
            string path = $"Scriptables/Status Effects/{original.name}";
            original = Resources.Load<StatusEffect>(path);
        }
        catch (InvalidPathException pathException)
        {
            Logger.LogException(pathException);
            throw;
        }
        StatusEffect effect = Object.Instantiate(original);
        
        return effect;
    }

    /// <summary>
    /// </summary>
    /// <param name="effects"></param>
    /// <returns>
    ///     Returns a tuple containing two lists of status effects. The first list contains status effects that apply
    ///     early (before damage is applied), and the second list contains status effects that apply late (after damage is
    ///     applied).
    /// </returns>
    public static (List<StatusEffect> appliesEarly, List<StatusEffect> appliesLate) Load(this List<StatusEffect> effects)
    {
        List<StatusEffect> appliesEarly = new();
        List<StatusEffect> appliesLate = new();
        
        foreach (var effect in effects)
        {
            if (effect.appliesEarly)
            {
                var instantiatedEffect = Load(effect);
                appliesEarly.Add(instantiatedEffect);
            }
            else
            {
                var instantiatedEffect = Load(effect);
                appliesLate.Add(instantiatedEffect);
            }
        }

        return (appliesEarly, appliesLate);
    }
    
    public static void Apply(this List<StatusEffect> effects, Entity target)
    {
        foreach (StatusEffect effect in effects)
        {
            effect.ApplyEffect(target);
        }
    }
}
