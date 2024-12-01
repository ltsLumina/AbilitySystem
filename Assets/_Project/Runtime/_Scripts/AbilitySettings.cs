#region
using System.Globalization;
using UnityEditor;
using UnityEngine;
#endregion

/// <summary>
///     Contains the default settings for abilities.
/// </summary>
public struct AbilitySettings
{
	/// <summary>
	///     The default GCD cooldown.
	/// </summary>
	public static float GlobalCooldown => 1.5f;

	/// <summary>
	///     <para> A DoT will deal damage every X tick cycles. </para>
	/// </summary>
	public static int DoT_Rate => 3;

	public struct ResourcePaths
	{
		public const string SCRIPTABLES = "Scriptables";
		public const string ABILITIES = "Scriptables/Abilities";
		public const string STATUS_EFFECTS = "Scriptables/Status Effects";
		public const string JOB = "Scriptables/Jobs";
	}

	public struct FullPaths
	{
		public const string SCRIPTABLES = "Assets/_Project/Runtime/_Scriptables";
		public const string ABILITIES = "Assets/_Project/Runtime/_Scriptables/Abilities";
		public const string STATUS_EFFECTS = "Assets/_Project/Runtime/Resources/Scriptables/Status Effects";
		public const string STATUS_EFFECTS_CS = "Assets/_Project/Runtime/_Scripts/Status Effects";
		public const string JOB = "Assets/_Project/Runtime/_Scriptables/Jobs";
	}
}

public static class ProjectSettingsProvider
{
	[SettingsProvider]
	public static SettingsProvider CreateProjectSettingsProvider()
	{
		var provider = new SettingsProvider("Project/Ability Settings", SettingsScope.Project)
		{ label = "Ability Settings",
		  guiHandler = searchContext =>
		  {
			  GUILayout.Label("Ability Settings", EditorStyles.boldLabel);
			  EditorGUILayout.LabelField("Global Cooldown", AbilitySettings.GlobalCooldown.ToString(CultureInfo.InvariantCulture));
			  EditorGUILayout.LabelField("DoT Rate", AbilitySettings.DoT_Rate.ToString());
		  } };

		return provider;
	}
}
