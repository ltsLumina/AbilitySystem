#region
using System;
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

    public enum Effects
    {
        None,
        DamageUp,
        DoT,
    }

    [SerializeField] [ReadOnly] string name;
    [SerializeField] [ReadOnly] int duration;
    [SerializeField] [ReadOnly] string description;
    [SerializeField] [ReadOnly] TargetType targets;
    [Tooltip("The time remaining for the status effect.")]
    [SerializeField, ReadOnly] float time;
    [Tooltip("The entity that applied the status effect. (Not the entity that the status effect is applied to.)")]
    [SerializeField, ReadOnly] Entity owner;

    public string Name => name;
    public int Duration => duration;
    public string Description => description;
    public TargetType Targets => targets;
    public float Time
    {
        get => time;
        set => time = value;
    }
    public Entity Owner => owner;

    public override string ToString() => $"{name} ({duration} seconds)";

    [Serializable]
    public class Effect : StatusEffect
    {
        public Effect(string name, int duration, string description, TargetType targets, Entity owner)
        {
            this.name = name;
            this.duration = duration;
            this.description = description;
            this.targets = targets;
            this.owner = owner;

            time = duration;
        }

        public class Buff : Effect // Mostly a marker class
        {
            public Buff(string name, int duration, string description, TargetType targets, Entity owner) : base(name, duration, description, targets, owner) { }
        }

        public class Debuff : Effect // Mostly a marker class
        {
            public Debuff(string name, int duration, string description, TargetType targets, Entity owner) : base(name, duration, description, targets, owner) { }
        }

        public struct Buffs
        {
            public static Buff DamageUp => All.DamageUp;
        }

        public struct Debuffs
        {
            public static Effect DoT => All.DoT;
        }

        public struct All
        {
            public static Buff DamageUp => new ("Damage Up", 10, "Increases damage dealt by 10%.", TargetType.Self, null);

            public static Effect DoT => new ("DoT", 24, "Deals X damage per second.", TargetType.Enemy, null);
        }

        public static Effect GetEffect(Effects statusEffect) => statusEffect switch
        { Effects.None     => null,
          Effects.DamageUp => Buffs.DamageUp,
          Effects.DoT      => Debuffs.DoT,
          _                => new ("None", 0, "No effect.", TargetType.Self, null) };
    }

    public void Invoke(Entity target, Entity caster)
    {
        // Effect logic here

        var effect = new Effect(name, duration, description, targets, caster);
        target.ApplyStatusEffects(effect);
    }

    public void Invoke(Effect overrideEffect, Entity target) =>

        // Effect logic here
        target.ApplyStatusEffects(overrideEffect);
}

public static class StatusEffectExtensions
{
}
