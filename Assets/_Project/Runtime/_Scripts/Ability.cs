#define USING_CUSTOM_INSPECTOR
#region
#if USING_CUSTOM_INSPECTOR && UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
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
        Q, // Primary
        W, // Secondary
        E, // Utility
        R, // Ultimate
    }

    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public enum DamageType
    {
        Direct,
        [Description("DoT")]
        DoT,
    }

    [Header("Ability Info"), SerializeField]
     Class abilityClass;
    [SerializeField] string abilityName;
    [TextArea, SerializeField]
     string abilityDescription;
    [SerializeField] public Sprite abilityIcon;
    [SerializeField] Key abilityKey;

    [Header("Ability Properties"), SerializeField]
     float range;
    [SerializeField] float radius;
    [SerializeField] bool usesCastTime;
    [SerializeField] float castTime;
    [SerializeField] bool usesGlobalCooldown;
    [SerializeField] float cooldown;

    [Header("Damage"), SerializeField]
     DamageType damageType;
    [SerializeField] float damage;
    [SerializeField] int damageCycles;

    List<Action> OnInvoke = new ();

    static Player player
    {
        get
        {
            var player = FindFirstObjectByType<Player>();
            return player;
        }
    }

    public static bool cancelled
    {
        get
        {
            player.GetComponentInChildren<InputManager>().TryGetComponent(out InputManager inputManager);
            return inputManager.MoveInput != Vector2.zero;
        }
    }

    public void Invoke()
    {
        Logger.Log("Ability has been invoked.");
        OnInvoke = InitializeOnInvoke();

        OnInvoke.ForEach(action => action.Invoke());
    }

    List<Action> InitializeOnInvoke()
    {
        Entity nearestTarget = null;
        bool isCast = false;
        bool isGCD = false;
        bool isDoT = false;

        Logger.LogBehaviour = Logger.LogLevel.Quiet;

        var actions = new List<Action>
        { () => Logger.LogExplicit("Ability has been invoked."),
          () =>
          {
              nearestTarget = FindClosestTarget();
              Logger.Log($"Nearest target: {nearestTarget}");
          },
          () =>
          {
              if (usesCastTime) isCast = true;
          },
          () =>
          {
              if (usesGlobalCooldown)
              {
                  Logger.Log($"Global cooldown: {cooldown} seconds.");
                  isGCD = true;
              }
          },
          () =>
          {
              // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
              switch (damageType)
              {
                  case DamageType.Direct:
                      Logger.Log($"Deals {damage} damage.");
                      break;

                  case DamageType.DoT:
                      Logger.Log($"Deals {damage} damage over {damageCycles} cycles.");
                      isDoT = true;
                      break;
              }
          },
          () =>
          {
              // Attack
              switch (true)
              {
                  case true when isCast:
                      Logger.Log("Casting...");
                      player.StartCoroutine(Cast());
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

              Logger.LogBehaviour = Logger.LogLevel.Verbose;
          } };

        return actions;
    }

    #region Casting
    Coroutine castCoroutine;

    IEnumerator Cast()
    {
        castCoroutine = player.StartCoroutine(CastCoroutine());
        yield return new WaitWhile(Casting);

        if (cancelled) yield break;

        // Cast the ability
        GameObject effect = Instantiate(Resources.Load<GameObject>("PREFABS/Effect"));
        effect.transform.position = player.transform.position + player.transform.right * 2.5f;
        effect.AddComponent<Rigidbody2D>().AddForce(player.transform.right * 500);
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
        TickManager.OnCycle += () => { damageable?.TakeDamage(damage); };

        // var tickManager = FindAnyObjectByType<TickManager>();
        // if (!tickManager) yield break;
        //
        // float cycleDuration = tickManager.TickCycleDuration;
        //
        // for (int i = 0; i < damageCycles; i++)
        // {
        //     yield return new WaitForSeconds(cycleDuration);
        //     // Apply damage
        //     target.TryGetComponent(out IDamageable damageable);
        //     damageable?.TakeDamage(damage);
        // }
    }

    [return: NotNull]
    static Entity FindClosestTarget()
    {
        Entity[] entities = FindObjectsByType<Entity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        // Find the closest entity to the player. If anything is null, throw an exception.
        return (player == null ? null : entities.Where(entity => entity != player).OrderBy(entity => Vector2.Distance(player.transform.position, entity.transform.position)).FirstOrDefault()) ??
               throw new InvalidOperationException();
    }
}

#if USING_CUSTOM_INSPECTOR
[CustomEditor(typeof(Ability), true), CanEditMultipleObjects]
public class AbilityEditor : Editor
{
    SerializedProperty abilityClass;
    SerializedProperty abilityName;
    SerializedProperty abilityDescription;
    SerializedProperty abilityIcon;
    SerializedProperty abilityKey;
    SerializedProperty range;
    SerializedProperty radius;
    SerializedProperty usesCastTime;
    SerializedProperty castTime;
    SerializedProperty usesGlobalCooldown;
    SerializedProperty cooldown;
    SerializedProperty damageType;
    SerializedProperty damage;
    SerializedProperty damageCycles;

    string abilityType => abilityKey.enumValueIndex switch
    { 0 => "Primary",
      1 => "Secondary",
      2 => "Utility",
      3 => "Ultimate",
      _ => "Unknown" };

    void OnEnable()
    {
        abilityClass = serializedObject.FindProperty("abilityClass");
        abilityName = serializedObject.FindProperty("abilityName");
        abilityDescription = serializedObject.FindProperty("abilityDescription");
        abilityIcon = serializedObject.FindProperty("abilityIcon");
        abilityKey = serializedObject.FindProperty("abilityKey");
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

        var centeredStyle = new GUIStyle(GUI.skin.label)
        { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 14 };

        using (new GUILayout.HorizontalScope("textField")) { showInfo = EditorGUILayout.BeginFoldoutHeaderGroup(showInfo, "Ability Info", centeredStyle); }

        if (showInfo)
            using (new GUILayout.VerticalScope("box"))
            {
                EditorGUILayout.PropertyField(abilityClass);
                EditorGUILayout.PropertyField(abilityName);
                EditorGUILayout.PropertyField(abilityDescription);
                EditorGUILayout.PropertyField(abilityIcon);
            }

        EditorGUILayout.EndFoldoutHeaderGroup();

        GUILayout.Space(25);

        using (new GUILayout.HorizontalScope("textField")) { showProperties = EditorGUILayout.BeginFoldoutHeaderGroup(showProperties, "Ability Properties", centeredStyle); }

        if (showProperties)
            using (new GUILayout.VerticalScope("box"))
            {
                EditorGUILayout.PropertyField(abilityKey);
                GUILayout.Label(abilityType, EditorStyles.centeredGreyMiniLabel);

                EditorGUILayout.PropertyField(range);
                EditorGUILayout.PropertyField(radius);

                EditorGUILayout.PropertyField(usesCastTime);

                if (usesCastTime.boolValue)
                {
                    EditorGUILayout.HelpBox("This ability uses a cast time.", MessageType.Info);
                    EditorGUILayout.PropertyField(castTime);
                }

                EditorGUILayout.PropertyField(usesGlobalCooldown);

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
                    damageCycles.intValue = EditorGUILayout.IntField("Number of Cycles", damageCycles.intValue);

                    // Clamp the duration to a minimum of 1 second.
                    damageCycles.intValue = Mathf.Max(1, damageCycles.intValue);

                    var tickManager = FindAnyObjectByType<TickManager>();
                    if (!tickManager) return;

                    int tickRate = tickManager.TickRate;
                    float cycleDuration = tickManager.TickCycleDuration;

                    float damagePerTick = damage.floatValue / tickRate;
                    float totalDamage = damage.floatValue * damageCycles.intValue;
                    float totalDuration = damageCycles.intValue * cycleDuration;

                    EditorGUILayout.LabelField("Damage per Tick", damagePerTick.ToString("F2"));
                    EditorGUILayout.LabelField("Total Damage", totalDamage.ToString("F0"));
                    EditorGUILayout.LabelField("Total Duration", totalDuration + "s");

                    if (damagePerTick > 2.75) EditorGUILayout.HelpBox("This ability deals a considerable amount of damage." + " \nConsider reducing the damage per cycle or the number of cycles.", MessageType.Warning);
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
        string newName = $"{abilityClass.enumDisplayNames[abilityClass.enumValueIndex]} [{abilityKey.enumDisplayNames[abilityKey.enumValueIndex]}] ({abilityType})";

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
