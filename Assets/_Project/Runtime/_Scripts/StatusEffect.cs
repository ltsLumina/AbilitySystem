#region
using System;
using System.Collections.Generic;
using System.Linq;
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

    public enum Effects
    {
        None,
        DamageUp,
        DoT,
    }

    [SerializeField] [ReadOnly] string statusName;
    [SerializeField] [ReadOnly] int duration;
    [SerializeField] [ReadOnly] string description;
    [SerializeField] [ReadOnly] TargetType targets;
    [SerializeField] public bool applyEarly;
    [Tooltip("The time remaining for the status effect.")]
    [SerializeField, ReadOnly] float time;
    [Tooltip("The entity that applied the status effect. (Not the entity that the status effect is applied to.)")]
    [SerializeField, ReadOnly] Entity owner;
    
    public string StatusName => statusName;
    public int Duration => duration;
    public string Description => description;
    public TargetType Targets => targets;
    public float Time
    {
        get => time;
        set => time = value;
    }
    public Entity Owner => owner;

    public override string ToString() => $"{statusName} ({duration} seconds)";

    public virtual void ApplyEffect(Entity target) => target.ApplyStatusEffects(this);

    public class Effect : StatusEffect
    {
        public Effect(string statusName, int duration, string description, TargetType targets, Entity owner)
        {
            this.statusName = statusName;
            this.duration = duration;
            this.description = description;
            this.targets = targets;
            this.owner = owner;

            time = duration;
        }
        
        public class Buff : Effect // Mostly a marker class
        {
            public Buff(string statusName, int duration, string description, TargetType targets, Entity owner) : base(statusName, duration, description, targets, owner) { }
        }

        public class Debuff : Effect // Mostly a marker class
        {
            public Debuff(string statusName, int duration, string description, TargetType targets, Entity owner) : base(statusName, duration, description, targets, owner) { }
        }

        public struct Buffs
        {
            public static Buff DamageUp => All.DamageUp;
        }

        public struct Debuffs
        {
            public static Debuff DoT => All.DoT;
        }

        public struct All
        {
            public static Buff DamageUp => new ("Damage Up", 10, "Increases damage dealt by 10%.", TargetType.Self, null);

            public static Debuff DoT => new ("DoT", 24, "Deals X damage per second.", TargetType.Enemy, null);
        }

        public static Effect GetEffect(Effects statusEffect) => statusEffect switch
        { Effects.None     => null,
          Effects.DamageUp => Buffs.DamageUp,
          Effects.DoT      => Debuffs.DoT,
          _                => new ("None", 0, "No effect.", TargetType.Self, null) };
    }
}

public static class StatusEffectExtensions
{
    public static StatusEffect LoadEffect(this StatusEffect statusEffect)
    {
        StatusEffect original; 
        
        try
        {
            original = Resources.Load<DamageUp>($"Scriptables/{statusEffect}");
        }
        catch (InvalidPathException pathException)
        {
            Logger.LogException(pathException);
            throw;
        }
        StatusEffect effect = Object.Instantiate(original);
        
        return effect;
    }
    
    public static void ApplyEffects(this List<StatusEffect> effects, Entity target, out List<StatusEffect> appliesEarly)
    {
        appliesEarly = new ();
        
        foreach (var effect in effects)
        {
            if (effect.applyEarly) appliesEarly.Add(effect);
            else effect.ApplyEffect(target);
        }
        
        // TODO: THIS
    }
}
