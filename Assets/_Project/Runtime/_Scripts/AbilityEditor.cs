#define USING_CUSTOM_INSPECTOR
#if USING_CUSTOM_INSPECTOR
#region
using System.Linq;
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
	SerializedProperty damage;
	SerializedProperty effects;

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
		damage = serializedObject.FindProperty("damage");
		effects = serializedObject.FindProperty("effects");
	}

	bool showInfo = true;
	bool showProperties = true;
	bool showAdditional;
	bool showTextures;
	int selectedEffect;
	TextAsset csv;
	int loadStage
	{
		get => EditorPrefs.GetInt("LoadStage", 0);
		set => EditorPrefs.SetInt("LoadStage", value);
	}

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

					damage.floatValue = EditorGUILayout.FloatField("Damage", Mathf.Clamp(damage.floatValue, 0f, 1000));
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

					EditorGUILayout.EndFoldoutHeaderGroup();

					GUILayout.Space(10);
					GUILayout.Label(string.Empty, GUI.skin.horizontalSlider);
					GUILayout.Space(20);

					StatusEffect[] statusEffects = Resources.LoadAll<StatusEffect>("Scriptables/Status Effects");
					string[] displayOptions = statusEffects.Select(e => ObjectNames.NicifyVariableName(e.name)).ToArray();
					displayOptions = displayOptions.Prepend("Select an Effect").ToArray();

					using (new EditorGUILayout.HorizontalScope())
					{
						csv = (TextAsset) EditorGUILayout.ObjectField("CSV File", csv, typeof(TextAsset), false);

						using (new EditorGUI.DisabledScope(Application.internetReachability == NetworkReachability.NotReachable))
						{
							GUIContent fetchContent = EditorGUIUtility.IconContent("Download-Available", "Fetch a CSV file from the web.");
							if (GUILayout.Button(fetchContent, GUILayout.Height(25), GUILayout.Width(50))) csv = CSVParser.FetchCSV();
						}
					}

					using (new EditorGUILayout.HorizontalScope())
					{
						using (new EditorGUI.DisabledScope(!csv || EditorApplication.isCompiling || EditorApplication.isUpdating))
						{
							var loadContent = new GUIContent("Load Effects", "Load status effects from a CSV file.");

							GUILayout.FlexibleSpace();

							if (GUILayout.Button(loadContent, GUILayout.Height(25), GUILayout.Width(270)))
							{
								// ReSharper disable once SuggestVarOrType_Elsewhere
								var effectDataList = CSVParser.Parse(csv);

								// Check if the CSV data was valid
								if (effectDataList != null)
								{
									// Write a script for each buff
									foreach ((string name, string description, string type, string duration, string target, string timing) effect in effectDataList)
										ScriptWriter.WriteScript(effect.name, effect.description, effect.type, effect.duration, effect.target, effect.timing);

									Logger.Log("C# scripts created successfully!");

									loadStage = loadStage == 0 ? 1 : 0;
								}
								else
								{
									Logger.LogError("Failed to parse CSV data.");
									loadStage = 0;
								}
							}

							var refreshIcon = new GUIContent(string.Empty, EditorTextures.All.Refresh);
							if (GUILayout.Button(refreshIcon, GUILayout.Width(50), GUILayout.Height(25))) loadStage = 0;
						}
					}

					using (new EditorGUILayout.HorizontalScope())
					{
						selectedEffect = EditorGUILayout.Popup("Add Status Effect", selectedEffect, displayOptions);

						using (new EditorGUI.DisabledScope(selectedEffect == 0))
						{
							if (GUILayout.Button("Add", GUILayout.Width(50)))
							{
								if (selectedEffect > 0)
								{
									StatusEffect effect = statusEffects[selectedEffect - 1];
									effects.InsertArrayElementAtIndex(effects.arraySize);
									SerializedProperty newEffect = effects.GetArrayElementAtIndex(effects.arraySize - 1);
									newEffect.objectReferenceValue = effect;
								}

								selectedEffect = 0;
								serializedObject.ApplyModifiedProperties();
							}
						}
					}

					EditorGUILayout.PropertyField(effects, true);

					if (effects.arraySize > 0)
					{
						EditorGUILayout.HelpBox("This ability applies status effects.", MessageType.Info);
						var infoContent = new GUIContent("The \"Duration\" field is controlled by the \"Cycles\" field above."); // TODO: THIS
						EditorGUILayout.LabelField(infoContent, EditorStyles.centeredGreyMiniLabel);

						// check if the inspector has been updated
						serializedObject.ApplyModifiedProperties();
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
}
#endif
