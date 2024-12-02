#region
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using JetBrains.Annotations;
using Lumina.Essentials.Attributes;
using Unity.Properties;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
#endregion

public abstract class StatusEffect : ScriptableObject
{
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	[Flags]
	protected enum Target
	{
		Self = 1,
		Enemy = 2,
		Ally = 4,
	}

	public enum Timing
	{
		Prefix,
		Postfix,
	}

	[SerializeField] protected string statusName;
	[TextArea, UsedImplicitly]
	[SerializeField] protected string description;
	[SerializeField] protected int duration;
	[SerializeField] protected Target target;
	[SerializeField] public Timing timing;
	[Tooltip("The time remaining for the status effect.")]
	[SerializeField] [ReadOnly] float time;
	[Tooltip("The entity that applied the status effect. (Not the entity that the status effect is applied to.)")]
	[SerializeField] [ReadOnly] protected Entity caster;

	public string StatusName => statusName;
	public int Duration => duration;
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

#if UNITY_EDITOR
	protected new string name => base.name = string.IsNullOrEmpty
			(Path.GetFileName(AssetDatabase.GetAssetPath(this)).Replace(".asset", string.Empty))
			? statusName
			: Path.GetFileName(AssetDatabase.GetAssetPath(this)).Replace(".asset", string.Empty);
#endif

	public override string ToString() => $"{statusName} ({duration} seconds)";

	protected Player player => FindAnyObjectByType<Player>();

	/// <summary>
	///     The entity that the status effect is applied to.
	/// </summary>
	protected Entity entity { get; private set; }

	#region Base
	/// <summary>
	/// </summary>
	/// <remarks>The base method for invoking a status effect must always be called in the derived class.</remarks>
	/// <param name="entityTarget"></param>
	public void Invoke(Entity entityTarget)
	{
		// set the target to the player if the target is self
		if (target == Target.Self) entityTarget = player;
		entity = entityTarget;

		entityTarget.AddStatusEffect(this);

		OnInvoke();
		Logger.Log($"Applied {this} to {entityTarget}");
	}

	Coroutine decayCoroutine;

	void Awake() => OnInstantiated += effect =>
	{
		effect.Time = effect.Duration;
		effect.decayCoroutine ??= CoroutineHelper.StartCoroutine(effect.Decay());
	};

	/// <summary>
	///     Coroutine for the status effect to decay over time.
	/// </summary>
	/// <returns></returns>
	IEnumerator Decay()
	{
		while (Time > 0)
		{
			Time -= UnityEngine.Time.deltaTime;
			yield return null;
		}

		OnDecay();
		OnDecayed?.Invoke(this);

		Destroy(this);
		decayCoroutine = null;
	}

	/// <summary>
	/// OnInvoke is called when the status effect is invoked.
	/// </summary>
	protected virtual void OnInvoke() { }
	/// <summary>
	/// OnDecay is called when the status effect has decayed.
	/// </summary>
	protected virtual void OnDecay() { }

	/// <summary>
	///     Callback for when a status effect has decayed.
	/// </summary>
	public event Action<StatusEffect> OnDecayed;

	public virtual void Reset()
	{
		statusName = "$NAME";
		description = "$DESCRIPTION";
		duration = 24;
		target = Target.Self;
		timing = Timing.Postfix;
	}

	public void Reapply()
	{
		entity.StopCoroutine(decayCoroutine);
		Time = Duration;
		decayCoroutine = CoroutineHelper.StartCoroutine(Decay());
	}

	/// <summary>
	///     The StatusEffect base class provides a method for applying damage over time to an entity.
	///     There are multiple effects that apply a DoT, so this method is provided to avoid code duplication.
	/// </summary>
	/// <param name="entityTarget"> The entity to apply the DoT to. </param>
	/// <param name="damage"> The amount of damage to apply. </param>
	/// <param name="onTick"> An optional callback for when the DoT ticks. </param>
	protected void DamageOverTime(Entity entityTarget, float damage, Action onTick = null)
	{
		// initial damage
		entityTarget.TryGetComponent(out IDamageable damageable);
		damageable?.TakeDamage(damage);

		int cycle = 0;
		int dotTick = 0;
		int dotTicks = duration / AbilitySettings.DoT_Rate - 1;

		// TODO: if the DoT is already running when it is re-applied, reset the cycle count.
		//  Probably will check if they have a debuff applied.
		TickManager.OnCycle += OnCycle;

		return;

		void OnCycle()
		{
			if (dotTick == dotTicks)
			{
				TickManager.OnCycle -= OnCycle;
				return;
			}

			cycle++;

			if (cycle % AbilitySettings.DoT_Rate == 0) // If DoT_Rate is 3, this will tick on cycle 3, 6, 9, etc.
			{
				damageable?.TakeDamage(damage);
				dotTick++;
				onTick?.Invoke();
			}
		}
	}
	#endregion

	#region Utility
	/// <summary>
	///     Callback for when a status effect is instantiated.
	///     The parameters are the status effect that was instantiated and the entity that the status effect was applied to.
	/// </summary>
	protected static event Action<StatusEffect> OnInstantiated;

	public static StatusEffect Instantiate(StatusEffect original)
	{
		StatusEffect effect = Object.Instantiate(original);

		OnInstantiated?.Invoke(effect);
		return effect;
	}
	#endregion
}

#region Effects
public abstract class Buff : StatusEffect // Just a marker class
{
}

public abstract class Debuff : StatusEffect // Just a marker class
{
}
#endregion

public static class StatusEffectExtensions
{
	/// <summary>
	/// </summary>
	/// <param name="original"> The status effect from the list of effects to load an instance of. </param>
	/// <returns> A runtime <see cref="ScriptableObject" /> instance of the Status Effect. </returns>
	static StatusEffect Load(StatusEffect original)
	{
		try
		{
			string path = $"{AbilitySettings.ResourcePaths.STATUS_EFFECTS}/{original.name}";
			original = Resources.Load<StatusEffect>(path);
		} catch (InvalidPathException pathException)
		{
			Logger.LogException(pathException);
			throw;
		}

		StatusEffect effect = StatusEffect.Instantiate(original);

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
		List<StatusEffect> prefix = new ();
		List<StatusEffect> postfix = new ();

		foreach (StatusEffect effect in effects)
		{
			switch (effect.timing)
			{
				case StatusEffect.Timing.Prefix: {
					StatusEffect instantiatedEffect = Load(effect);
					prefix.Add(instantiatedEffect);
					break;
				}

				case StatusEffect.Timing.Postfix: {
					StatusEffect instantiatedEffect = Load(effect);
					postfix.Add(instantiatedEffect);
					break;
				}
			}
		}

		return (prefix, postfix);
	}

	public static void Apply(this List<StatusEffect> effects, (Entity target, Entity caster) target)
	{
		foreach (StatusEffect effect in effects)
		{
			effect.Caster = target.caster;
			effect.Invoke(target.target);
		}
	}
}
