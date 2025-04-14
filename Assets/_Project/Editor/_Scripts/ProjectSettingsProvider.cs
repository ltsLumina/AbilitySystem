#region
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#endregion

public static class ProjectSettingsProvider
{
	[SettingsProvider]
	public static SettingsProvider CreateMyCustomSettingsProvider()
	{
		// First parameter is the path in the Settings window.
		// Second parameter is the scope of this setting: it only appears in the Project Settings window.
		var provider = new SettingsProvider("Project/AbilitySettings", SettingsScope.Project)
		{ // By default the last token of the path is used as display name if no label is provided.
		  label = "Ability Settings",

		  // Create the SettingsProvider and initialize its drawing (IMGUI) function in place:
		  guiHandler = searchContext =>
		  {
			  var headerButtonStyle = new GUIStyle(GUI.skin.label)
			  { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 14 };

			  using (new GUILayout.HorizontalScope("textField")) { EditorGUILayout.BeginFoldoutHeaderGroup(true, "Ability Info", headerButtonStyle); }

			  GUILayout.Space(15);

			  SerializedObject settings = AbilitySettings.GetSerializedSettings();
			  EditorGUILayout.PropertyField(settings.FindProperty("globalCooldown"));
			  EditorGUILayout.PropertyField(settings.FindProperty("dotRate"));
			  EditorGUILayout.PropertyField(settings.FindProperty("damageVariance"));
			  EditorGUILayout.PropertyField(settings.FindProperty("critChance"));
			  EditorGUILayout.PropertyField(settings.FindProperty("critMultiplier"));
			  settings.ApplyModifiedPropertiesWithoutUndo();

			  EditorGUILayout.EndFoldoutHeaderGroup();
		  },

		  // Populate the search keywords to enable smart search filtering and label highlighting:
		  keywords = new HashSet<string>
		  (new[]
		   { "Global Cooldown", "DoT" }) };

		return provider;
	}
}
