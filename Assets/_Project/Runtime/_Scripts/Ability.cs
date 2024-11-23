#define USING_CUSTOM_INSPECTOR
#region
#if USING_CUSTOM_INSPECTOR && UNITY_EDITOR
using UnityEditor;
using UnityEngine.InputSystem;
#endif
using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static Job;
#endregion

public interface IAbility
{
    void Invoke();
}

[CreateAssetMenu(fileName = "New Ability", menuName = "Abilities/Ability")]
public sealed class Ability : ScriptableObject, IAbility
{
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    enum Key
    {
        [InspectorName("Primary")]
        Q, // Primary
        [InspectorName("Secondary")]
        W, // Secondary
        [InspectorName("Utility")]
        E, // Utility
        [InspectorName("Ultimate")]
        R, // Ultimate
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
    [SerializeField] string abilityDescription;
    [SerializeField] public Sprite abilityIcon;
    [SerializeField] Key abilityType;

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
    [SerializeField] int damageCycles;

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
            player.GetComponentInChildren<InputManager>().TryGetComponent(out InputManager inputManager);
            return inputManager.MoveInput != Vector2.zero;
        }
    }

    public override string ToString() => abilityName == string.Empty ? name : abilityName;

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
        int totalCycles = damageCycles / AbilitySettings.DoT_Cycles;

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

#if USING_CUSTOM_INSPECTOR
[CustomEditor(typeof(Ability), true), CanEditMultipleObjects]
public class AbilityEditor : Editor
{
    SerializedProperty job;
    SerializedProperty abilityName;
    SerializedProperty abilityDescription;
    SerializedProperty abilityIcon;
    SerializedProperty abilityType;
    SerializedProperty range;
    SerializedProperty radius;
    SerializedProperty usesCastTime;
    SerializedProperty castTime;
    SerializedProperty usesGlobalCooldown;
    SerializedProperty cooldown;
    SerializedProperty damageType;
    SerializedProperty damage;
    SerializedProperty damageCycles;

    #region Properties
    Player player => FindFirstObjectByType<Player>();

    bool isDirect => damageType.enumValueIndex == 0;
    bool isDoT => damageType.enumValueIndex == 1;

    /// <summary>
    ///     Gets the job name based on the ability class enum value.
    ///     <example> Reaper, Red Mage, Dark Knight, Sage, Developer </example>
    /// </summary>
    string jobName => job.enumValueIndex switch
    { 0 => "Reaper",
      1 => "Red Mage",
      2 => "Dark Knight",
      3 => "Sage",
      4 => "Developer",
      _ => "Unknown" };

    /// <summary>
    ///     Gets the job tri-code based on the ability class enum value.
    ///     <example> RPR, RDM, DRK, SGE, DEV </example>
    /// </summary>
    string jobTriCode => job.enumValueIndex switch
    { 0 => "RPR",
      1 => "RDM",
      2 => "DRK",
      3 => "SGE",
      4 => "DEV",
      _ => "Unknown" };

    /// <summary>
    ///     Gets the long version of the ability type based on the ability type enum value.
    ///     <example> Primary, Secondary, Utility, Ultimate </example>
    /// </summary>
    string abilityTypeNoun => abilityType.enumValueIndex switch
    { 0 => "Primary",
      1 => "Secondary",
      2 => "Utility",
      3 => "Ultimate",
      _ => "Unknown" };

    /// <summary>
    ///     Get the short version of the ability type.
    ///     <example> Q, W, E, R </example>
    /// </summary>
    string abilityTypeKey
    {
        get
        {
            var playerInput = player.GetComponentInChildren<PlayerInput>();

            return abilityType.enumValueIndex switch
            { 0 => GetBindingDisplayString(0),
              1 => GetBindingDisplayString(1),
              2 => GetBindingDisplayString(2),
              3 => GetBindingDisplayString(3),
              _ => "Unknown" };

            string GetBindingDisplayString(int abilityIndex) => $"{playerInput.actions[InputManager.AbilityKeys[abilityIndex]].GetBindingDisplayString()}";
        }
    }
    #endregion

    void OnEnable()
    {
        job = serializedObject.FindProperty("job");
        abilityName = serializedObject.FindProperty("abilityName");
        abilityDescription = serializedObject.FindProperty("abilityDescription");
        abilityIcon = serializedObject.FindProperty("abilityIcon");
        abilityType = serializedObject.FindProperty("abilityType");
        range = serializedObject.FindProperty("range");
        radius = serializedObject.FindProperty("radius");
        usesCastTime = serializedObject.FindProperty("usesCastTime");
        castTime = serializedObject.FindProperty("castTime");
        usesGlobalCooldown = serializedObject.FindProperty("usesGlobalCooldown");
        cooldown = serializedObject.FindProperty("cooldown");
        damageType = serializedObject.FindProperty("damageType");
        damage = serializedObject.FindProperty("damage");
        damageCycles = serializedObject.FindProperty("damageCycles");
    }

    bool showInfo = true;
    bool showProperties = true;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var headerButtonStyle = new GUIStyle(GUI.skin.label)
        { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 14 };

        using (new GUILayout.HorizontalScope("textField")) { showInfo = EditorGUILayout.BeginFoldoutHeaderGroup(showInfo, "Ability Info", headerButtonStyle); }

        if (showInfo)
            using (new GUILayout.VerticalScope("box"))
            {
                EditorGUILayout.PropertyField(job);
                EditorGUILayout.LabelField(jobName, EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.PropertyField(abilityName);
                EditorGUILayout.PropertyField(abilityDescription);
                EditorGUILayout.PropertyField(abilityIcon);
            }

        EditorGUILayout.EndFoldoutHeaderGroup();

        GUILayout.Space(25);

        using (new GUILayout.HorizontalScope("textField")) { showProperties = EditorGUILayout.BeginFoldoutHeaderGroup(showProperties, "Ability Properties", headerButtonStyle); }

        if (showProperties)
            using (new GUILayout.VerticalScope("box"))
            {
                EditorGUILayout.PropertyField(abilityType);
                GUILayout.Label($"Key: {abilityTypeKey}", EditorStyles.centeredGreyMiniLabel);

                EditorGUILayout.PropertyField(range);
                EditorGUILayout.PropertyField(radius);

                using (new EditorGUI.DisabledGroupScope(isDoT))
                {
                    if (isDoT) usesCastTime.boolValue = false;
                    EditorGUILayout.PropertyField(usesCastTime);
                }

                if (usesCastTime.boolValue)
                {
                    EditorGUILayout.HelpBox("This ability uses a cast time.", MessageType.Info);
                    EditorGUILayout.PropertyField(castTime);
                }
                else { castTime.floatValue = 0; }

                using (new EditorGUI.DisabledGroupScope(isDoT))
                {
                    if (isDoT) usesGlobalCooldown.boolValue = false;
                    EditorGUILayout.PropertyField(usesGlobalCooldown);
                }

                if (usesGlobalCooldown.boolValue)
                {
                    EditorGUILayout.HelpBox("This ability is on the global cooldown.", MessageType.Info);

                    using (new EditorGUI.DisabledGroupScope(true))
                    {
                        cooldown.floatValue = AbilitySettings.GlobalCooldown;
                        EditorGUILayout.PropertyField(cooldown);
                    }
                }
                else { EditorGUILayout.PropertyField(cooldown); }

                EditorGUILayout.PropertyField(damageType);

                if (damageType.enumValueIndex == 0)
                {
                    EditorGUILayout.HelpBox("This ability deals direct damage.", MessageType.Info);
                    EditorGUILayout.PropertyField(damage);
                }
                else
                {
                    EditorGUILayout.HelpBox("This ability deals damage over time.", MessageType.Info);

                    damage.floatValue = EditorGUILayout.FloatField("Damage per Cycle", damage.floatValue);
                    damageCycles.intValue = EditorGUILayout.IntField("Cycles (seconds)", damageCycles.intValue);

                    // Clamp the duration to a minimum of 1 second.
                    damageCycles.intValue = Mathf.Max(1, damageCycles.intValue);

                    var tickManager = FindAnyObjectByType<TickManager>();
                    if (!tickManager) return;

                    int tickRate = tickManager.TickRate;
                    float damagePerTick = damage.floatValue / tickRate;
                    float totalDamage = damage.floatValue * damageCycles.intValue;

                    EditorGUILayout.LabelField("Damage Per Tick", damagePerTick.ToString("F2"));
                    EditorGUILayout.LabelField("Total Damage", totalDamage.ToString("F0"));
                    EditorGUILayout.LabelField("DoT Cycles", $"{damageCycles.intValue / AbilitySettings.DoT_Cycles}");

                    EditorGUILayout.LabelField
                    ($"DoTs deal damage every {AbilitySettings.DoT_Cycles} tick cycles. Therefore this DoT will deal damage {damageCycles.intValue / AbilitySettings.DoT_Cycles} times.",
                     EditorStyles.centeredGreyMiniLabel);

                    if (damagePerTick > 2.75) EditorGUILayout.HelpBox("This DoT deals a considerable amount of damage." + " \nConsider reducing the damage per cycle or the number of cycles.", MessageType.Warning);
                }
            }

        EditorGUILayout.EndFoldoutHeaderGroup();

        if (serializedObject.ApplyModifiedProperties()) RenameAbilityAsset();
    }

    // TODO: Implement this method
    /// <summary>
    ///     Find the average damage dealt by a GCD ability
    /// </summary>
    /// <returns></returns>

    // int AverageGCDRotation()
    // {
    //     // Find a GCD ability
    //     var gcdAbility = FindObjectsOfType<Ability>().FirstOrDefault(a => a.abilityKey == Ability.Key.Q);
    //     
    //     const int time = 10;
    //     float averageDamage = (gcdAbility.damage * gcdAbility.cooldown) * time;
    //     return averageDamage;
    // }
    void RenameAbilityAsset()
    {
        var ability = (Ability) target;
        string newName = $"{jobTriCode} [{abilityTypeKey}] ({abilityTypeNoun})";
        string assetPath = AssetDatabase.GetAssetPath(ability);
        AssetDatabase.RenameAsset(assetPath, newName);
        ability.name = newName;
        AssetDatabase.SaveAssets();
    }
}
#endif

#if USING_CUSTOM_INSPECTOR
[CustomPropertyDrawer(typeof(Ability.DamageType))]
public class DamageTypeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Get the enum type
        Type enumType = fieldInfo.FieldType;

        // Get the enum names and their descriptions
        string[] enumNames = Enum.GetNames(enumType);
        string[] enumDescriptions = new string[enumNames.Length];

        for (int i = 0; i < enumNames.Length; i++)
        {
            object enumValue = Enum.Parse(enumType, enumNames[i]);
            enumDescriptions[i] = GetEnumDescription((Enum) enumValue);
        }

        // Get the current index
        int currentIndex = property.enumValueIndex;
        currentIndex = Mathf.Clamp(currentIndex, 0, enumDescriptions.Length - 1);

        // Display the popup with descriptions
        int newIndex = EditorGUI.Popup(position, label.text, currentIndex, enumDescriptions);
        if (newIndex != currentIndex) property.enumValueIndex = newIndex;
    }

    string GetEnumDescription(Enum value)
    {
        FieldInfo fieldInfo = value.GetType().GetField(value.ToString());
        var descriptionAttributes = (DescriptionAttribute[]) fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
        return descriptionAttributes.Length > 0 ? descriptionAttributes[0].Description : value.ToString();
    }
}
#endif
