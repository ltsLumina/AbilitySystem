#region
using System;
using System.Collections;
using System.ComponentModel;
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
    public enum AbilityType
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

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum DamageType
    {
        Direct,
        [Description("DoT")] // Unity will "nicify" this to "Do T" unless I use the Description attribute.
        DoT,
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
    [SerializeField] DamageType damageType;
    [SerializeField] float damage;
    [SerializeField] int duration;

    public static event Action OnGlobalCooldown;

    public Class Job => job;
    public string Name
    {
        get => abilityName;
        set => abilityName = value;
    }
    public string Description => description;
    public Sprite Icon => icon;
    public AbilityType Type => type;
    public float Range => range;
    public float Radius => radius;
    public CooldownType CDType => cooldownType;
    public float CastTime => castTime;
    public float Cooldown => cooldown;
    public DamageType DmgType => damageType;
    public float Damage => damage;
    public int Duration => duration;

    static Player _player;

    static Player player
    {
        get
        {
            if (!_player) _player = FindFirstObjectByType<Player>();
            return _player;
        }
    }

    public bool cancellable => cooldownType == CooldownType.Cast;

    public bool cancelled
    {
        get
        {
            InputManager inputManager = player.Inputs;
            return inputManager.MoveInput != Vector2.zero;
        }
    }

    public override string ToString() => abilityName == string.Empty ? this.name : abilityName;

    public void Invoke()
    {
        Entity nearestTarget = FindClosestTarget();
        bool isGCD = cooldownType == CooldownType.GCD;
        bool isCast = cooldownType == CooldownType.Cast;
        bool isInstant = cooldownType == CooldownType.Instant;
        bool isDoT = damageType == DamageType.DoT;

        switch (true)
        {
            case true when isDoT: // DoT needs to be handled first as it can be all types of cooldown types.
                Logger.Log("Applying DoT...");
                DamageOverTime(nearestTarget);
                break;

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

        Effect(target);

        target.TryGetComponent(out IDamageable damageable);
        damageable?.TakeDamage(damage);
    }

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

        Effect(target);

        target.TryGetComponent(out IDamageable damageable);
        damageable?.TakeDamage(damage);
    }

    void Instant(Entity target)
    {
        // Cast the ability
        Effect(target);

        target.TryGetComponent(out IDamageable damageable);
        damageable?.TakeDamage(damage);
    }

    void DamageOverTime(Entity target)
    {
        OnGlobalCooldown?.Invoke();

        Effect(target);

        target.TryGetComponent(out IDamageable damageable);
        damageable?.TakeDamage(damage, new StatusEffect("DoT", 24));

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
                Effect(target, true);
                damageable?.TakeDamage(damage);
                dotTick++;
            }
        }
    }

    static GameObject GetPooledObject(GameObject prefab) => ObjectPoolManager.FindObjectPool(prefab, 5).GetPooledObject(true);

    static void Effect(Entity target, bool isDoT = false)
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
}
