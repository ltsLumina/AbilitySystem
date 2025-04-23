#region
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DG.Tweening;
using Lumina.Essentials.Attributes;
using UnityEngine;
using static Job;
using Random = UnityEngine.Random;
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
	[Tooltip("Whether the ability is on cooldown when a new battle begins.")]
	[SerializeField] bool startsOnCooldown;

	[Header("Damage Properties")]
	[SerializeField] float damage;
	[Tooltip("If a value is set, this ability will overcharge the selected ability.")]
	[SerializeField] Ability abilityToPrime;
	[SerializeField] float primeChance;
	[SerializeField] Ability abilityToRefund;
	[SerializeField] float refundChance;

	[Tooltip("The status effects that this ability applies.")]
	[SerializeField] List<StatusEffect> effects;

	public static event Action<Player> OnGlobalCooldown;

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
	public float CastTime => castTime * caster.Stats.CastSpeed;
	public float Cooldown
	{
		get
		{
			if (!caster) return cooldown;
			return cooldown * caster.Stats.SpellSpeed;
		}
	}
	public bool StartsOnCooldown => startsOnCooldown;
	public float Damage => damage;

	Player caster;

	/// <returns> Returns true if the ability is actively being cast, otherwise false. </returns>
	public bool casting => castCoroutine != null;

	/// <returns> Returns true if the ability uses a cast time, otherwise false. </returns>
	public bool cancellable => cooldownType == CooldownType.Cast;

	public bool cancelled
	{
		get
		{
			InputManager inputManager = caster.InputManager;
			return inputManager.MoveInput != Vector2.zero;
		}
	}

	public override string ToString() => abilityName == string.Empty ? name : abilityName;

	void OnDisable()
	{ /* not called when Disable Domain Reload is active. */
	}

	public enum InvokeOutcome
	{
		Success,
		NoTarget,
	}

	public bool Invoke(Player owner, out InvokeOutcome reason)
	{
		caster = owner;

		Entity nearestTarget = FindBoss();
		if (nearestTarget == null)
		{
			reason = InvokeOutcome.NoTarget;
			return false;
		}

		bool isGCD = cooldownType == CooldownType.GCD;
		bool isCast = cooldownType == CooldownType.Cast;
		bool isInstant = cooldownType == CooldownType.Instant;

		Attack();

		reason = InvokeOutcome.Success;
		return true;
		Boss FindBoss()
		{
			Boss[] allBosses = FindObjectsByType<Boss>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
			Boss nearest = allBosses.Where(b => b != null && b.gameObject.activeInHierarchy).OrderBy(b => Vector2.Distance(caster.transform.position, b.transform.position)).FirstOrDefault();

#if UNITY_EDITOR && false
			if (nearest == null)
			{
				var scarecrow = Resources.Load<Boss>("PREFABS/Bosses/Scarecrow");
				Boss instance = Instantiate(scarecrow, Vector3.zero, Quaternion.identity);
				instance.gameObject.SetActive(true);
				return instance;
			}
#endif

			return nearest;
		}

		void Attack()
		{
			if (nearestTarget.TryGetComponent(out Boss boss) && boss.IsDead)
			{
				// If the boss is dead, do not apply the ability
				return;
			}
			
			switch (true)
			{
				case true when isGCD:
					GlobalCooldown(nearestTarget);
					break;

				case true when isCast:
					caster.StartCoroutine(Cast(nearestTarget));
					break;

				case true when isInstant:
					Instant(nearestTarget);
					break;
			}
		}
	}

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

		while (elapsedTime < CastTime)
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
		OnGlobalCooldown?.Invoke(caster);

		ApplyEffects(target);
	}

	IEnumerator Cast(Entity target)
	{
		OnGlobalCooldown?.Invoke(caster);

		if // is primed, skip casting
			(primedStates.TryGetValue(caster, out bool isPrimed) && isPrimed) { /* skip */ }
		else // is not primed, start casting
		{
			castCoroutine = caster.StartCoroutine(CastCoroutine());
			var particle = Instantiate(Resources.Load<GameObject>("PREFABS/Casting Particles")).GetComponent<ParticleSystem>();
			particle.transform.SetParent(caster.transform);
			ParticleSystem.MainModule particleMain = particle.main;
			particleMain.duration = CastTime * 2 - 0.35f;
			particle.transform.position = caster.transform.position + new Vector3(0, -0.8f);
			particle.Play();
			yield return new WaitWhile(Casting);

			if (cancelled)
			{
				particle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
				yield break;
			}
		}

		ApplyEffects(target);
	}

	void Instant(Entity target)
	{
		ApplyEffects(target);
	}

	void ApplyEffects(Entity target)
	{
		VisualEffect(target);

		(List<StatusEffect> prefix, List<StatusEffect> postfix) = effects.Load();

		target.TryGetComponent(out IDamageable enemy);

		float variance = DamageVariance();
		float chargedMult = IsPrimed(caster);

		float distance = Vector2.Distance(caster.transform.position, target.transform.position);
		float critMult = distance < range ? 1.5f : DetermineCriticalHit();

		SetPrimed(caster, false); // note: can't remember why this is here but its probably important
		
		float damageBeforePlayerStats = damage * variance * critMult;
		float damageWithPlayerStats = damageBeforePlayerStats * caster.Stats.Damage;
		float finalDamage = damageWithPlayerStats * chargedMult;
		
		if (prefix.Count > 0) prefix.Apply((target, caster));
		enemy.TakeDamage(finalDamage);
		if (postfix.Count > 0) postfix.Apply((target, caster));

		bool sameValues = Mathf.Approximately(primeChance, refundChance);
		if (sameValues)
		{
			float chance = Mathf.Max(primeChance, refundChance);

			if (Proc(chance))
			{
				if (abilityToPrime) abilityToPrime.SetPrimed(caster, true);
				if (abilityToRefund) SetPendingRefund(caster, true);
			}

			return;
		}
		
		if (abilityToPrime)
			if (Proc(primeChance))
				abilityToPrime.SetPrimed(caster, true);

		if (abilityToRefund)
			if (Proc(refundChance)) SetPendingRefund(caster, true);
	}

	#region Utility for Prime / Refund
	static bool Proc(float chance)
	{
		if (chance <= 0) return false;

		float randomValue = Random.Range(0f, 1f);
		return randomValue <= chance;
	}

	readonly Dictionary<Player, bool> primedStates = new ();
	readonly Dictionary<Player, bool> refundStates = new ();

	void SetPrimed(Player player, bool value) => primedStates[player] = value;
	
	/// <returns> 1.25f if the ability is primed, otherwise 1. </returns>
	public float IsPrimed(Player player)
	{
		const float primedMultiplier = 1.25f;

		if (player == null) return 1f;
		
		if (primedStates.TryGetValue(player, out bool isPrimed) && isPrimed) 
			return primedMultiplier;

		return 1;
	}

	void SetPendingRefund(Player player, bool value) => refundStates[player] = value;

	public bool TryRefund(Player player, out Ability ability)
	{
		if (refundStates.TryGetValue(player, out bool pending) && pending)
		{
			SetPendingRefund(player, false);
			ability = abilityToRefund;
			return true;
		}

		ability = null;
		return false;
	}

	/// <summary>
	///     Resets the properties of the ability such as primed and refund states.
	/// </summary>
	public void ResetProperties()
	{
		primedStates.Clear();
		refundStates.Clear();
	}

	float DamageVariance()
	{
		RangedFloat defaultVariance = AbilitySettings.DamageVariance;
		float variance = defaultVariance.GetRandomValue();
		return variance;
	}

	/// <summary>
	///     Determines if the ability is a critical hit.
	///     If it is, returns the critical hit multiplier.
	/// </summary>
	/// <returns> AbilitySettings.CritMultiplier (default = 1.5x) if the ability is a critical hit, otherwise 1. </returns>
	float DetermineCriticalHit()
	{
		float critChance = AbilitySettings.CritChance;
		float critMultiplier = AbilitySettings.CritMultiplier;

		if (Random.Range(0f, 1f) <= critChance)
		{
			return critMultiplier;
		}

		return 1;
	}
	#endregion

	static void VisualEffect(Entity target, bool isDoT = false)
	{
		var prefab = Resources.Load<GameObject>("PREFABS/Effect");
		GameObject instantiate = Instantiate(prefab, target.transform.position, Quaternion.identity);
		instantiate.transform.localScale = isDoT ? new (0.5f, 0.5f) : new (1, 1);
		var sprite = instantiate.GetComponent<SpriteRenderer>();

		sprite.DOFade(0, 1).OnComplete(() => { sprite.DOFade(1, 0).SetLink(instantiate).OnComplete(() => { Destroy(instantiate); }); });
	}
}
