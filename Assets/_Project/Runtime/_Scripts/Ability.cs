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

	public static event Action OnGlobalCooldown;

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

	static Player _player;

	static Player player
	{
		get
		{
			if (!_player) _player = FindFirstObjectByType<Player>();
			return _player;
		}
	}

	/// <returns> Returns true if the ability is actively being cast, otherwise false. </returns>
	public bool casting => castCoroutine != null;

	/// <returns> Returns true if the ability uses a cast time, otherwise false. </returns>
	public bool cancellable => cooldownType == CooldownType.Cast;

	public bool cancelled
	{
		get
		{
			InputManager inputManager = player.Inputs;
			return inputManager.MoveInput != Vector2.zero;
		}
	}

	public override string ToString() => abilityName == string.Empty ? name : abilityName;

	void OnDisable()
	{ /* not called when Disable Domain Reload is active. */
	}

	public void Invoke()
	{
		Entity nearestTarget = FindClosestTarget();
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
				player.StartCoroutine(Cast(nearestTarget));
				break;

			case true when isInstant:
				Logger.Log("Instant cast...");
				Instant(nearestTarget);
				break;
		}

		return;

		[return: NotNull]
		static Entity FindClosestTarget()
		{
			Entity[] entities = FindObjectsByType<Entity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

			// Find the closest entity to the player. If anything is null, throw an exception.
			return (player == null ? null : entities.Where(entity => entity != player).OrderBy(entity => Vector2.Distance(player.transform.position, entity.transform.position)).FirstOrDefault()) ??
			       throw new InvalidOperationException();
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
		OnGlobalCooldown?.Invoke();
		
		ApplyEffects(target);
	}

	IEnumerator Cast(Entity target)
	{
		OnGlobalCooldown?.Invoke();

		castCoroutine = player.StartCoroutine(CastCoroutine());
		var particle = Instantiate(Resources.Load<GameObject>("PREFABS/Casting Particles")).GetComponent<ParticleSystem>();
		ParticleSystem.MainModule particleMain = particle.main;
		particleMain.duration = castTime + 0.5f;
		particle.transform.position = player.transform.position + new Vector3(0, -0.8f);
		particle.Play();
		yield return new WaitWhile(Casting);

		if (cancelled) yield break;

		ApplyEffects(target);
	}

	void Instant(Entity target) => ApplyEffects(target);

	void ApplyEffects(Entity target)
	{
		VisualEffect(target);

		(List<StatusEffect> prefix, List<StatusEffect> postfix) = effects.Load();

		target.TryGetComponent(out IDamageable enemy);

		if (prefix.Count > 0) prefix.Apply((target, player));
		enemy?.TakeDamage(damage);
		if (postfix.Count > 0) postfix.Apply((target, player));
	}

	static void VisualEffect(Entity target, bool isDoT = false)
	{
		var prefab = Resources.Load<GameObject>("PREFABS/Effect");
		GameObject pooled = GetPooledObject();
		pooled.transform.position = target.transform.position;
		pooled.transform.localScale = isDoT ? new (0.5f, 0.5f) : new (1, 1);
		var sprite = pooled.GetComponent<SpriteRenderer>();

		sprite.DOFade(0, 1).OnComplete
		(() =>
		{
			sprite.DOFade(1, 0);
			pooled.SetActive(false);
		});

		return;

		GameObject GetPooledObject() => ObjectPoolManager.FindObjectPool(prefab, 5).GetPooledObject(true);
	}
}
