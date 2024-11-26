#region
using System;
using System.Collections.Generic;
using System.Linq;
using Lumina.Essentials.Attributes;
using UnityEngine;
using Object = UnityEngine.Object;
#endregion

[Serializable]
public class StatusEffect<T>
    where T : Effect
{
    [SerializeField] T effect;
    [SerializeField] [ReadOnly] string name;
    [SerializeField] [ReadOnly] string description;
    [SerializeField] [ReadOnly] float duration;
    [SerializeField, ReadOnly] float time;
    [SerializeField] [ReadOnly] Entity caster;

    public string Name => name;
    public string Description => description;
    public float Duration => duration;
    public float Time
    {
        get => time;
        set => time = value;
    }
    public Entity Caster
    {
        get => caster;
        set => caster = value;
    }

    public T Effect => effect;
}

[Serializable]
public abstract class Effect : Object
{
    public float Potency { get; private set; }

    public abstract class Buff : Effect
    {
        public class ArcaneCircle : Buff
        {
            public ArcaneCircle() { Invoke(); }

            public void Invoke()
            {
                // Implementation for increasing damage
                Debug.Log("Damage increased.");
                Potency = 1.5f;
            }
        }

        public void Invoke<T>()
            where T : Buff
        {
            (this as T)?.Invoke<T>();
            Debug.Log("cool?");
        }

        // public void Invoke<T>() where T : Buff, new()
        // {
        //     new T().Invoke<T>();
        // }
    }

    public abstract class Debuff : Effect
    {
        public class DamageDown : Debuff
        {
            public void Invoke() =>

                // Implementation for decreasing damage
                Debug.Log("Damage decreased.");
        }
    }

    public void Invoke() => (this as Buff)?.Invoke();
}

public static class StatusEffectExtensions
{
    public static List<StatusEffect<Effect>> GetEffects<T>(this Entity entity) => entity.StatusEffects.Where(e => e.Effect is T).ToList();

    public static List<StatusEffect<Effect>> GetEffects<T>(this List<StatusEffect<Effect>> effects) => effects.Where(e => e.Effect is T).ToList();

    public static void InvokeAll(this List<StatusEffect<Effect>> effects)
    {
        foreach (StatusEffect<Effect> effect in effects) effect.Effect?.Invoke();

        //(effect.Effect as Effect.Debuff)?.Invoke<Effect.Debuff>();
    }
}
