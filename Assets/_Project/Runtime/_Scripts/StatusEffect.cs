#region
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using DG.Tweening;
using JetBrains.Annotations;
using Lumina.Essentials.Attributes;
using Unity.Properties;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
#endregion

public class StatusEffect : ScriptableObject
{
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	[Flags]
	public enum Target
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

	protected Player casterAsPlayer => caster.TryGetComponent(out Player player) ? player : null;

#if UNITY_EDITOR
	protected new string name => base.name = string.IsNullOrEmpty(Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(this))) ? statusName : Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(this));
#endif

	public override string ToString() => $"{statusName} ({duration} seconds)";

	/// <summary>
	///     The entity that the status effect is applied to.
	/// </summary>
	protected Entity entity { get; private set; }

	public static StatusEffect CreateCustomStatusEffect(string name, string description, int duration, Target target, Timing timing, Entity caster = null)
	{
		var customEffect = CreateInstance<StatusEffect>();
		customEffect.statusName = name;
		customEffect.description = description;
		customEffect.duration = duration;
		customEffect.target = target;
		customEffect.timing = timing;
		customEffect.caster = caster;

		customEffect = Instantiate(customEffect);
		return customEffect;
	}

	#region Base
	/// <summary>
	/// </summary>
	/// <remarks>The base method for invoking a status effect must always be called in the derived class.</remarks>
	public void Invoke(Entity _)
	{
		List<Entity> targets = new ();

		// If target includes Self
		if ((target & Target.Self) != 0) targets.Add(caster);

		// If target includes Ally
		if ((target & Target.Ally) != 0)
		{
			// Example of finding allies (adjust as needed)
			List<Entity> allies = FindObjectsByType<Player>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Where(p => p != casterAsPlayer).Cast<Entity>().ToList();

			targets.AddRange(allies);
		}

		if (target == Target.Enemy)
		{
			Boss boss = GameManager.Instance.CurrentBoss;
			if (boss) targets.Add(boss);
		}

		// Now apply the status effect to each target
		foreach (Entity t in targets)
		{
			if (t.HasStatusEffect(this, out StatusEffect existingEffect))
			{
				// Reset the existing effect's remaining time
				existingEffect.Time = Duration;
				existingEffect.Caster = Caster;
				Logger.Log($"Reapplied {this} to {t}");
			}
			else
			{
				entity = t;
				t.AddStatusEffect(this);
				OnInvoke();
				OnInvoked?.Invoke(this);
				Time = Duration;
				decayCoroutine ??= CoroutineHelper.StartCoroutine(Decay());
				Logger.Log($"Applied {this} to {t}");
			}
		}
	}

	Coroutine decayCoroutine;

	// void Awake() => OnInstantiated += effect =>
	// {
	// 	effect.Time = effect.Duration;
	// 	effect.decayCoroutine ??= CoroutineHelper.StartCoroutine(effect.Decay());
	// };

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
	///     Callback for when a status effect has decayed.
	/// </summary>
	protected virtual void OnDecay() { }
	public event Action<StatusEffect> OnDecayed;

	protected virtual void OnInvoke() { }
	public event Action<StatusEffect> OnInvoked;

	public virtual void Reset()
	{
		statusName = "$NAME";
		description = "$DESCRIPTION";
		duration = 24;
		target = Target.Self;
		timing = Timing.Postfix;
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
