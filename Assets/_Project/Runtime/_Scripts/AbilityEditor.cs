#define USING_CUSTOM_INSPECTOR
#if USING_CUSTOM_INSPECTOR
#region
using System;
using System.ComponentModel;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
#endregion

[CustomEditor(typeof(Ability), true)] [CanEditMultipleObjects]
public class AbilityEditor : Editor
{
    SerializedProperty job;
    SerializedProperty abilityName;
    SerializedProperty description;
    SerializedProperty icon;
    SerializedProperty type;
    SerializedProperty range;
    SerializedProperty radius;
    SerializedProperty cooldownType;
    SerializedProperty castTime;
    SerializedProperty cooldown;
    SerializedProperty damageType;
    SerializedProperty damage;
    SerializedProperty duration;

    #region Properties
    static Player player => FindAnyObjectByType<Player>();

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
    string abilityTypeNoun => type.enumValueIndex switch
    { 0 => "Primary",
      1 => "Secondary",
      2 => "Utility",
      3 => "Ultimate",
      _ => "Unknown" };

    /// <summary>
    ///     Get the short version of the ability type.
    ///     <example> Q, W, E, R </example>
    /// </summary>
    string abilityTypeKey => $"{player.PlayerInput.actions[InputManager.AbilityKeys[type.enumValueIndex]].GetBindingDisplayString()}";
    #endregion

    void OnEnable()
    {
        job = serializedObject.FindProperty("job");
        abilityName = serializedObject.FindProperty("abilityName");
        description = serializedObject.FindProperty("description");
        icon = serializedObject.FindProperty("icon");
        type = serializedObject.FindProperty("type");
        range = serializedObject.FindProperty("range");
        radius = serializedObject.FindProperty("radius");
        cooldownType = serializedObject.FindProperty("cooldownType");
        castTime = serializedObject.FindProperty("castTime");
        cooldown = serializedObject.FindProperty("cooldown");
        damageType = serializedObject.FindProperty("damageType");
        damage = serializedObject.FindProperty("damage");
        duration = serializedObject.FindProperty("duration");
    }

    bool showInfo = true;
    bool showProperties = true;
    bool showAdditional;
    bool showTextures;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var headerButtonStyle = new GUIStyle(GUI.skin.label)
        { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 14 };

        using (new GUILayout.HorizontalScope("textField")) { showInfo = EditorGUILayout.BeginFoldoutHeaderGroup(showInfo, "Ability Info", headerButtonStyle); }

        {
            if (showInfo)
                using (new GUILayout.VerticalScope("box"))
                {
                    EditorGUILayout.PropertyField(job);
                    EditorGUILayout.LabelField(jobName, EditorStyles.centeredGreyMiniLabel);
                    EditorGUILayout.PropertyField(abilityName);
                    EditorGUILayout.PropertyField(description);
                    EditorGUILayout.PropertyField(icon);
                }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        GUILayout.Space(25);

        using (new GUILayout.HorizontalScope("textField")) { showProperties = EditorGUILayout.BeginFoldoutHeaderGroup(showProperties, "Ability Properties", headerButtonStyle); }

        {
            if (showProperties)
            {
                using (new GUILayout.VerticalScope("box"))
                {
                    EditorGUILayout.PropertyField(type);
                    GUILayout.Label($"Key: {abilityTypeKey}", EditorStyles.centeredGreyMiniLabel);

                    range.floatValue = EditorGUILayout.Slider("Range", range.floatValue, 0f, 25);
                    radius.floatValue = EditorGUILayout.Slider("Radius", radius.floatValue, 0f, 25);

                    EditorGUILayout.PropertyField(cooldownType, new GUIContent("Cooldown Type", "Does this ability use the global cooldown (GCD & Cast)? Or is it instant?"));

                    switch (cooldownType.enumValueIndex)
                    {
                        case 0: // GCD

                            using (new EditorGUI.DisabledScope(true))
                            {
                                EditorGUILayout.PropertyField(cooldown);
                                cooldown.floatValue = AbilitySettings.GlobalCooldown;
                            }

                            EditorGUILayout.HelpBox("This ability is on the global cooldown.", MessageType.Info);

                            castTime.floatValue = 0;
                            break;

                        case 1: // Instant
                            EditorGUILayout.PropertyField(cooldown);
                            EditorGUILayout.HelpBox("This ability is instant and does not use the global cooldown.", MessageType.Info);

                            castTime.floatValue = 0;
                            break;

                        case 2: // Cast
                            castTime.floatValue = EditorGUILayout.FloatField("Cast Time", Mathf.Clamp(castTime.floatValue, 0f, 5));
                            EditorGUILayout.HelpBox("This ability uses a cast time.", MessageType.Info);

                            EditorGUILayout.PropertyField(cooldown);
                            if (cooldown.floatValue <= 1) EditorGUILayout.HelpBox("This ability has a very short cooldown.", MessageType.Warning);
                            break;
                    }

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
                        duration.intValue = EditorGUILayout.IntField("Cycles (seconds)", Mathf.Clamp(duration.intValue, 3, 60));
                        if (duration.intValue < 9) EditorGUILayout.HelpBox("DoT has a very short duration.", MessageType.Warning);

                        int tickRate = TickManager.Instance.TickRate;
                        float damagePerTick = damage.floatValue / tickRate;
                        float totalDamage = damage.floatValue * duration.intValue;

                        EditorGUILayout.LabelField("Damage Per Tick", damagePerTick.ToString("F2"));
                        EditorGUILayout.LabelField("Total Damage", totalDamage.ToString("F0"));
                        EditorGUILayout.LabelField("DoT Ticks", $"{duration.intValue / AbilitySettings.DoT_Rate}");

                        var dotInfoContent = new GUIContent($"DoTs deal damage every {AbilitySettings.DoT_Rate} tick cycles. Therefore this DoT will deal damage {duration.intValue / AbilitySettings.DoT_Rate} times.");
                        EditorGUILayout.LabelField(dotInfoContent, EditorStyles.centeredGreyMiniLabel);

                        if (damagePerTick > 2.75) EditorGUILayout.HelpBox("This DoT deals a considerable amount of damage." + " \nConsider reducing the damage per cycle or the number of cycles.", MessageType.Warning);
                    }
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        GUILayout.Space(25);
        using (new GUILayout.HorizontalScope("textField")) { showAdditional = EditorGUILayout.BeginFoldoutHeaderGroup(showAdditional, "Additional Information", headerButtonStyle); }

        if (showAdditional)
            using (new GUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("Debugging Information", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Job Name", jobName);
                EditorGUILayout.LabelField("Job Tri-Code", jobTriCode);
                EditorGUILayout.LabelField("Ability Type", abilityTypeNoun);
                EditorGUILayout.LabelField("Ability Key", abilityTypeKey);
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

        if (!string.IsNullOrEmpty(assetPath) && ability.name != newName)
        {
            AssetDatabase.RenameAsset(assetPath, newName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            ability.name = newName;
        }
    }

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
}
#endif
