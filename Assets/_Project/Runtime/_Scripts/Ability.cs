#region
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DG.Tweening;
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
	public float CastTime => castTime;
	public float Cooldown => cooldown;
	public float Damage => damage;

	Player localPlayer;

	/// <returns> Returns true if the ability is actively being cast, otherwise false. </returns>
	public bool casting => castCoroutine != null;

	/// <returns> Returns true if the ability uses a cast time, otherwise false. </returns>
	public bool cancellable => cooldownType == CooldownType.Cast;

	public bool cancelled
	{
		get
		{
			InputManager inputManager = localPlayer.Inputs;
			return inputManager.MoveInput != Vector2.zero;
		}
	}

	public override string ToString() => abilityName == string.Empty ? name : abilityName;

	void OnDisable()
	{ /* not called when Disable Domain Reload is active. */
	}

	public void Invoke(Player owner)
	{
		localPlayer = owner;

		Entity nearestTarget = FindBoss();
		bool isGCD = cooldownType == CooldownType.GCD;
		bool isCast = cooldownType == CooldownType.Cast;
		bool isInstant = cooldownType == CooldownType.Instant;

		Attack();

		return;

		Boss FindBoss()
		{
			Boss[] allBosses = FindObjectsByType<Boss>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
			Boss nearest = allBosses.Where(b => b != null && b.gameObject.activeInHierarchy).OrderBy(b => Vector2.Distance(localPlayer.transform.position, b.transform.position)).FirstOrDefault();

			if (nearest == null)
			{
				// TODO: change to find dummy class.
				var scarecrow = FindAnyObjectByType<Scarecrow>(FindObjectsInactive.Include).GetComponent<Boss>();
				scarecrow!.gameObject.SetActive(true);
				return scarecrow;
			}

			return nearest;
		}

		void Attack()
		{
			switch (true)
			{
				case true when isGCD:
					GlobalCooldown(nearestTarget);
					break;

				case true when isCast:
					localPlayer.StartCoroutine(Cast(nearestTarget));
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
		OnGlobalCooldown?.Invoke(localPlayer);

		ApplyEffects(target);
	}

	IEnumerator Cast(Entity target)
	{
		OnGlobalCooldown?.Invoke(localPlayer);

		castCoroutine = localPlayer.StartCoroutine(CastCoroutine());
		var particle = Instantiate(Resources.Load<GameObject>("PREFABS/Casting Particles")).GetComponent<ParticleSystem>();
		ParticleSystem.MainModule particleMain = particle.main;
		particleMain.duration = castTime + 0.5f;
		particle.transform.position = localPlayer.transform.position + new Vector3(0, -0.8f);
		particle.Play();
		yield return new WaitWhile(Casting);

		if (cancelled) yield break;

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

		if (enemy == null)
		{
			Logger.LogError($"{target} is not damageable.");
			return;
		}

		if (prefix.Count > 0) prefix.Apply((target, localPlayer));
		enemy.TakeDamage(damage * localPlayer.Stats.Damage);
		if (postfix.Count > 0) postfix.Apply((target, localPlayer));
	}

	static void VisualEffect(Entity target, bool isDoT = false)
	{
		var prefab = Resources.Load<GameObject>("PREFABS/Effect");
		GameObject instantiate = Instantiate(prefab, target.transform.position, Quaternion.identity);
		instantiate.transform.localScale = isDoT ? new (0.5f, 0.5f) : new (1, 1);
		var sprite = instantiate.GetComponent<SpriteRenderer>();

		sprite.DOFade(0, 1).OnComplete(() => { sprite.DOFade(1, 0).SetLink(instantiate).OnComplete(() => { Destroy(instantiate); }); });
	}
}
