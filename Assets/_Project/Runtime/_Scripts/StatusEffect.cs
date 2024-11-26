#region
using System;
using System.Collections.Generic;
using Lumina.Essentials.Attributes;
using UnityEngine;
#endregion

[Serializable]
public class StatusEffect
{
    public enum TargetType
    {
        Self,
        Enemy,
    }
    
    [SerializeField] string name;
    [SerializeField] int duration;
    [SerializeField] string description;
    [SerializeField] TargetType target;
    [Tooltip("The time remaining for the status effect.")]
    [SerializeField, ReadOnly] float time;
    [Tooltip("The entity that applied the status effect. (Not the entity that the status effect is applied to.)")]
    [SerializeField, ReadOnly] Entity owner;
    
    public string Name => name;
    public int Duration
    {
        get => duration;
        set => duration = value;
    }
    public string Description => description;
    public float Time
    {
        get => time;
        set => time = value;
    }
    public Entity Owner
    {
        get => owner;
        set => owner = value;
    }
    public TargetType Target => target;

    public override string ToString()
    {
        return $"{name} ({duration} seconds)";
    }

    public StatusEffect(Entity owner, Effect effect)
    {
        this.owner = owner;
        name = effect.Name;
        duration = effect.Duration;
        description = effect.Description;
        time = duration;
    }

    public class Effect
    {
        public enum Effects
        {
            None,
            DamageUp,
            DoT,
        }
        
        public string Name { get; }
        public int Duration { get; }
        public string Description { get; }

        public Effect(string name, int duration, string description)
        {
            Name = name;
            Duration = duration;
            Description = description;
        }

        public override string ToString() => Name;

        public static Effect GetEffect(Effects effect)
        {
            return effect switch
            {
                Effects.DamageUp => Buffs.DamageUp(),
                Effects.DoT => Debuffs.DoT(),
                _ => null
            };
        }
    }

    public struct Buffs
    {
        public static Effect DamageUp(int duration = 30) => new Effect("Damage Up", duration, "Increases damage dealt.");
    }

    public struct Debuffs
    {
        public static Effect DoT(int duration = 30) => new Effect("Damage Over Time", duration, "Deals damage over time.");
    }
}

public static class StatusEffectExtensions
{
    /// <summary>
    /// Tries to get the first status effect in the entity's status effects list.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns> Returns the first status effect in the entity's status effects list. </returns>
    public static StatusEffect TryGetStatusEffect(this Entity entity) => entity.StatusEffects.Count > 0 ? entity.StatusEffects[0] : null;

    
    /// <summary>
    /// Gets the status effects of the entity.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns> All the status effects of the entity. </returns>
    public static List<StatusEffect> GetStatusEffects(this Entity entity) => entity.StatusEffects;
}