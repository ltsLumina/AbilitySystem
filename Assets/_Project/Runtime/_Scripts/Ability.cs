#region
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;
using static Job;
#endregion

[CreateAssetMenu(fileName = "New Ability", menuName = "Abilities/Ability")]
public sealed class Ability : ScriptableObject
{
	[SuppressMessage("ReSharper", "UnusedMember.Local")]
	enum AbilityType // Marker enum
	{
		Primary,
		Secondary,
		Utility,
		Ultimate,
	}

	public enum CooldownType
	{
		GCD,     // Weaponskill
		Instant, // Ability (oGCD)
		Cast,    // Spell
	}

	[Header("Ability Info")]

	[SerializeField] Class job;
	[SerializeField] string abilityName;
	[TextArea]
	[SerializeField] string description;
	[SerializeField] Sprite icon;
	[SerializeField] AbilityType type;

	[Header("Ability Properties")]

	[SerializeField] float range;
	[SerializeField] float radius;
	[SerializeField] CooldownType cooldownType;
	[SerializeField] float castTime;
	[SerializeField] float cooldown;

	[Header("Damage Properties")]

	[SerializeField] float damage;

	[Tooltip("The status effects that this ability applies.")]
	[SerializeField] List<StatusEffect> effects;

	public static event Action OnGlobalCooldown;

	public Player owner { get; set; } // AKA: the player using the ability
	public Class Job => job;
	public string Name
	{
		get => abilityName;
		set => abilityName = value;
	}
	public string Description => description;
	public Sprite Icon => icon;
	public float Range => range;
	public float Radius => radius;
	public CooldownType CDType => cooldownType;
	public float CastTime => castTime;
	public float Cooldown => cooldown;
	public float Damage => damage;

	public bool cancellable => cooldownType == CooldownType.Cast;

	public bool cancelled
	{
		get
		{
			InputManager inputManager = owner.Inputs;
			return inputManager.MoveInput != Vector2.zero;
		}
	}

	public override string ToString() => abilityName == string.Empty ? name : abilityName;

	public void Invoke()
	{
		Entity nearestTarget = FindNearestTarget();
		bool isGCD = cooldownType == CooldownType.GCD;
		bool isCast = cooldownType == CooldownType.Cast;
		bool isInstant = cooldownType == CooldownType.Instant;

		switch (true)
		{
			case true when isGCD:
				Logger.Log("On global cooldown.");
				GlobalCooldown(nearestTarget);
				break;

			case true when isCast:
				Logger.Log("Casting...");
				owner.StartCoroutine(Cast(nearestTarget));
				break;

			case true when isInstant:
				Logger.Log("Instant cast...");
				Instant(nearestTarget);
				break;
		}

		return;

		[return: NotNull]
		Entity FindNearestTarget()
		{
			Entity[] entities = FindObjectsByType<Entity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Where(entity => entity is not Player && entity != owner).ToArray();

			// Find the closest entity to the player. If anything is null, throw an exception.
			return (owner == null ? null : entities.Where(entity => entity != owner).OrderBy(entity => Vector2.Distance(owner.transform.position, entity.transform.position)).FirstOrDefault()) ?? throw new InvalidOperationException();
		}
	}

	// ReSharper disable once Unity.RedundantEventFunction
	void OnDisable() { } // Won't be called with "Disable Domain Reload" enabled.

	#region Casting
	Coroutine castCoroutine;

	/// <summary>
	///     Check if the ability is actively being cast.
	/// </summary>
	/// <returns> Returns true if the ability is actively being cast, otherwise false. </returns>
	bool Casting() => castCoroutine != null;

	IEnumerator CastCoroutine()
	{
		float elapsedTime = 0f;

		while (elapsedTime < castTime)
		{
			if (cancelled)
			{
				castCoroutine = null;
				yield break;
			}

			elapsedTime += Time.deltaTime;
			yield return null;
		}

		castCoroutine = null;
	}
	#endregion

	void GlobalCooldown(Entity target)
	{
		OnGlobalCooldown?.Invoke();

		ApplyEffects(target);
	}

	IEnumerator Cast(Entity target)
	{
		OnGlobalCooldown?.Invoke();

		castCoroutine = owner.StartCoroutine(CastCoroutine());
		var particle = Instantiate(Resources.Load<GameObject>("PREFABS/Casting Particles")).GetComponent<ParticleSystem>();
		ParticleSystem.MainModule particleMain = particle.main;
		particleMain.duration = castTime + 0.5f;
		particle.transform.position = owner.transform.position + new Vector3(0, -0.8f);
		particle.Play();
		yield return new WaitWhile(Casting);

		if (cancelled) yield break;

		ApplyEffects(target);
	}

	void Instant(Entity target) => ApplyEffects(target);
	
	void ApplyEffects(Entity target)
	{
		(List<StatusEffect> prefix, List<StatusEffect> postfix) = effects.Load();

		target.TryGetComponent(out IDamageable enemy);

		if (prefix.Count > 0) prefix.Apply((target, owner));
		enemy?.TakeDamage(damage);
		VisualEffect(target);
		if (postfix.Count > 0) postfix.Apply((target, owner));
	}

	#region Visual Effects
	static GameObject GetPooledObject(GameObject prefab) => ObjectPoolManager.FindObjectPool(prefab, 5).GetPooledObject(true);
	
	static void VisualEffect(Entity target, bool isDoT = false)
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

	void DamageOverTime(Entity target)
	{
		OnGlobalCooldown?.Invoke();

		target.TryGetComponent(out IDamageable damageable);
		damageable?.TakeDamage(damage);

		int cycle = 0;
		int dotTick = 0;
		int dotTicks = effects.Find(effect => effect.StatusName == "DoT").Duration / AbilitySettings.DoT_Rate - 1;

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
			}
		}
	}
}
