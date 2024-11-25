#region
using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum DamageType
    {
        Direct,
        [Description("DoT")] // Unity will "nicify" this to "Do T" unless I use the Description attribute.
        DoT,
    }

    [Header("Ability Info")]
    [SerializeField] public Class job;
    [SerializeField] string abilityName;
    [TextArea]
    [SerializeField] string description;
    [SerializeField] public Sprite icon;
    [SerializeField] AbilityType type;

    [Header("Ability Properties")]
    [SerializeField] float range;
    [SerializeField] float radius;
    [SerializeField] bool usesCastTime;
    [SerializeField] public float castTime;
    [SerializeField] public bool usesGlobalCooldown;
    [SerializeField] public float cooldown;

    [Header("Damage Properties")]
    [SerializeField] DamageType damageType;
    [SerializeField] float damage;
    [SerializeField] int damageTicks;

    static Player _player;

    static Player player
    {
        get
        {
            if (!_player) _player = FindFirstObjectByType<Player>();
            return _player;
        }
    }

    public bool cancellable => usesCastTime;

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
        bool isCast = usesCastTime;
        bool isGCD = usesGlobalCooldown;
        bool isDoT = damageType == DamageType.DoT;

        switch (true)
        {
            case true when isCast:
                Logger.Log("Casting...");
                player.StartCoroutine(Cast(nearestTarget));
                break;

            case true when isGCD:
                Logger.Log("On global cooldown.");
                GlobalCooldown(nearestTarget);
                break;

            case true when isDoT:
                Logger.Log("Applying DoT...");
                DamageOverTime(nearestTarget);
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

    IEnumerator Cast(Entity target = null)
    {
        castCoroutine = player.StartCoroutine(CastCoroutine());
        yield return new WaitWhile(Casting);

        if (cancelled) yield break;

        // Cast the ability
        GameObject effect = Instantiate(Resources.Load<GameObject>("PREFABS/Effect"));
        effect.transform.position = player.transform.position + player.transform.right * 2.5f;
        effect.AddComponent<Rigidbody2D>().AddForce(player.transform.right * 500);

        if (!target) yield break;
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

    void GlobalCooldown(Entity target = null)
    {
        // Cast the ability
        GameObject effect = Instantiate(Resources.Load<GameObject>("PREFABS/Effect"));
        var player = GameObject.FindGameObjectWithTag("Player");
        effect.transform.position = player.transform.position + player.transform.right * 2.5f;
        effect.AddComponent<Rigidbody2D>().AddForce(player.transform.right * 500);

        if (target == null) return;
        target.TryGetComponent(out IDamageable damageable);
        damageable?.TakeDamage(damage);
    }

    void DamageOverTime(Entity target)
    {
        target.TryGetComponent(out IDamageable damageable);
        int cycle = 0;
        int totalCycles = damageTicks / AbilitySettings.DoT_Cycles;

        TickManager.OnCycle += OnCycle;
        return;

        void OnCycle()
        {
            if (cycle >= totalCycles)
            {
                TickManager.OnCycle -= OnCycle;
                return;
            }

            cycle++;

            if (cycle == 3)
            {
                damageable?.TakeDamage(damage);
                cycle = 0;
            }
        }
    }
}
