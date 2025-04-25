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

	public virtual void TakeDamage(float damage) => Logger.Log($"{name} took {damage} damage.", this);

	#region Status Effects
	public void AddStatusEffect(StatusEffect effect)
	{
		effect.OnDecayed += e => statusEffects.Remove(e);

		statusEffects.Add(effect);
	}

	public void RemoveStatusEffect(StatusEffect effect) // Note: I have no clue if this actually works.
	{
		if (statusEffects.Contains(effect))
		{
			statusEffects.Remove(effect);
			effect.OnDecayed -= e => statusEffects.Remove(e);
		}
		//else Debug.LogWarning($"{name} does not have the status effect {effect.StatusName}.");
	}

	public void RemoveStatusEffect(string effectName) // Note: I have no clue if this actually works.
	{
		StatusEffect effect = statusEffects.FirstOrDefault(e => e.StatusName == effectName);

		if (effect != null)
		{
			statusEffects.Remove(effect);
			effect.OnDecayed -= e => statusEffects.Remove(effect);
		}
		else Debug.LogWarning($"{name} does not have the status effect {effectName}.");
	}

	/// <summary>
	///     Cleanses all status effects from the entity.
	///     <remarks> Does not call OnDecayed on the status effects.</remarks>
	/// </summary>
	public void Cleanse()
	{
		foreach (StatusEffect effect in statusEffects)
		{
			if (effect == null) continue;

			effect.OnDecayed -= e => statusEffects.Remove(e);
		}

		statusEffects.Clear();
	}

	public bool HasStatusEffect(StatusEffect effect, out StatusEffect existingEffect)
	{
		existingEffect = statusEffects.FirstOrDefault(e => e.StatusName == effect.StatusName);
		return existingEffect != null;
	}
	#endregion

	public override string ToString() => $"{name} ({GetType().Name}) \n";

	bool hasTicked;

	protected IEnumerator Start()
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
			OnStart();

			// any other initialization code here
			OnEntityEnable += e => { Logger.Log($"{e.name} has been enabled."); };

			OnEntityDisable += e => { Logger.Log($"{e.name} has been disabled."); };

			OnEntityDestroy += e => { Logger.Log($"{e.name} has been destroyed."); };
		}
	}

	protected virtual void OnStart() { }

	protected virtual void OnTick() { }

	protected virtual void OnCycle() { }

	int tickCycles;

	protected virtual void OnTriggerEnter2D(Collider2D other) => Debug.Log($"{name} caused a trigger collision with: {other.gameObject.name}.");

	protected virtual void OnCollisionEnter2D(Collision2D other) => Debug.Log($"{name} collided with {other.gameObject.name}.");
}
