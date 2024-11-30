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

	#region Events
	public event Action<Entity> OnEntityEnable;
	public event Action<Entity> OnEntityDisable;
	public event Action<Entity> OnEntityDestroy;
	#endregion

	void IEntity.OnEnable() => OnEntityEnable?.Invoke(this);

	void IEntity.OnDisable() => OnEntityDisable?.Invoke(this);

	void IEntity.OnDestroy() => OnEntityDestroy?.Invoke(this);

	public virtual void TakeDamage(float damage) => Debug.Log($"{name} took {damage} damage.");

	public void ApplyStatusEffects(StatusEffect effect)
	{
		StatusEffect existingEffect = statusEffects.FirstOrDefault(e => e.StatusName == effect.StatusName);

		if (existingEffect)
		{
			if (existingEffect.Caster != effect.Caster) statusEffects.Add(effect);
			else existingEffect.Time = effect.Duration;
		}
		else { statusEffects.Add(effect); }
	}

	public override string ToString() => $"{name} ({GetType().Name}) \n";

	bool hasTicked;

	protected virtual IEnumerator Start()
	{
		Initialize();

		TaskCompletionSource<bool> tickTask = InitializeTick();

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
			OnEntityEnable += e => { Logger.Log($"{e.name} has been enabled."); };

			OnEntityDisable += e => { Logger.Log($"{e.name} has been disabled."); };

			OnEntityDestroy += e => { Logger.Log($"{e.name} has been destroyed."); };
		}
	}

	public void Remove(StatusEffect effect) => statusEffects.Remove(effect);

	protected abstract void OnTick();

	protected abstract void OnCycle();

	int tickCycles;

	void OnCollisionEnter2D(Collision2D other)
	{
		if (!other.gameObject.CompareTag("Player")) Debug.Log($"{name} collided with {other.gameObject.name}.");
	}
}
