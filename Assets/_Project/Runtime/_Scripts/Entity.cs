#region
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;
#endregion

/// <summary>
///     <para>
///         An entity is a game object that can interact with other entities.
///         It is the base class for all entities in the game, and should be inherited from by all entities.
///         A game object is not an entity unless it inherits from this class.
///     </para>
/// </summary>
public abstract class Entity : MonoBehaviour, IEntity, IDamageable
{
    [SerializeField] protected List<StatusEffect> statusEffects = new ();

    public enum ActiveState
    {
        Enabled,
        Disabled,
    }

    public ActiveState State { get; private set; }

    #region Events
    public event Action<Entity> OnEntityEnable;
    public event Action<Entity> OnEntityDisable;
    public event Action<Entity> OnEntityDestroy;
    #endregion

    void IEntity.OnEnable() => OnEntityEnable?.Invoke(this);

    void IEntity.OnDisable() => OnEntityDisable?.Invoke(this);

    void IEntity.OnDestroy() => OnEntityDestroy?.Invoke(this);

    public virtual void TakeDamage(float damage) => Debug.Log($"{name} took {damage} damage.");

    public void ApplyStatusEffects(params StatusEffect[] effects)
    {
        foreach (StatusEffect effect in effects)
        {
            var existingEffect = statusEffects.FirstOrDefault(e => e.StatusName == effect.StatusName);

            if (existingEffect != null)
            {
                if (existingEffect.Owner != effect.Owner) statusEffects.Add(effect);
                else existingEffect.Time = effect.Duration;
            }
            else { statusEffects.Add(effect); }
        }
    }

    public override string ToString() => $"{name} ({GetType().Name}) \n" + $"State: {State}";

    bool hasTicked;

    protected virtual IEnumerator Start()
    {
        Initialize();

        var tickTask = InitializeTick();

        yield return new WaitForSeconds(1);
        Debug.Assert(tickTask.Task.IsCompleted, $"{name} has not ticked after 1 second. " + "\nEnsure the TickManager is running and that your entity is calling base.Start() in its Start method.");

        yield break;

        TaskCompletionSource<bool> InitializeTick()
        {
            var tcs = new TaskCompletionSource<bool>();

            TickManager.OnTick += () =>
            {
                OnTick();
                tcs.TrySetResult(true);
            };

            TickManager.OnCycle += OnCycle;

            return tcs;
        }

        void Initialize()
        {
            // any other initialization code here
            OnEntityEnable += e =>
            {
                Logger.Log($"{e.name} has been enabled.");
                State = ActiveState.Enabled;
            };

            OnEntityDisable += e =>
            {
                Logger.Log($"{e.name} has been disabled.");
                State = ActiveState.Disabled;
            };

            OnEntityDestroy += e => { Logger.Log($"{e.name} has been destroyed."); };
        }
    }

    void Update()
    {
        foreach (StatusEffect.Effect effect in statusEffects.ToList())
        {
            effect.Time -= Time.deltaTime;
            if (effect.Time <= 0) statusEffects.Remove(effect);
        }
    }

    protected abstract void OnTick();

    protected abstract void OnCycle();

    int tickCycles;

    void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Player")) { Debug.Log($"{name} collided with {other.gameObject.name}."); }
    }
}
