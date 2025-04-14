#region
using Lumina.Essentials.Attributes;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
#endregion

/// <summary>
///     Contains the default settings for abilities.
/// </summary>
public class AbilitySettings : ScriptableObject
{
	const string SETTINGS_PATH = "Assets/_Project/Editor/AbilitySettings.asset";

#pragma warning disable CS0414 // Field is assigned but its value is never used
	[SerializeField] float globalCooldown = 1.5f;
	[SerializeField] int dotRate = 3;
	[RangedFloat(0.9f, 1.15f)]
	[SerializeField] RangedFloat damageVariance = new (0.95f, 1.1f);
	[SerializeField] float critChance = 0.05f;
	[SerializeField] float critMultiplier = 1.5f;
#pragma warning restore CS0414 // Field is assigned but its value is never used

#if UNITY_EDITOR
	static AbilitySettings GetOrCreateSettings()
	{
		var settings = AssetDatabase.LoadAssetAtPath<AbilitySettings>(SETTINGS_PATH);

		if (settings == null)
		{
			settings = CreateInstance<AbilitySettings>();
			settings.globalCooldown = 1.5f;
			settings.dotRate = 3;
			settings.damageVariance = new RangedFloat(0.9f, 1.1f);
			settings.critChance = 0.05f;
			settings.critMultiplier = 1.5f;

			AssetDatabase.CreateAsset(settings, SETTINGS_PATH);
			AssetDatabase.SaveAssets();
		}

		return settings;
	}

	public static SerializedObject GetSerializedSettings() => new (GetOrCreateSettings());
#endif

#if UNITY_EDITOR
	public static float GlobalCooldown => GetOrCreateSettings().globalCooldown;
	public static int DoT_Rate => GetOrCreateSettings().dotRate;
	public static RangedFloat DamageVariance => GetOrCreateSettings().damageVariance;
	public static float CritChance => GetOrCreateSettings().critChance;
	public static float CritMultiplier => GetOrCreateSettings().critMultiplier;
#else
 public static float GlobalCooldown => 1.5f;
 public static int DoT_Rate => 3;
 public static RangedFloat DamageVariance => new RangedFloat(0.9f, 1.1f);
	public static float CritChance => 0.05f;
	public static float CritMultiplier => 1.5f;
#endif

	public struct ResourcePaths
	{
		public const string SCRIPTABLES = "Scriptables";
		public const string ABILITIES = "Scriptables/Abilities";
		public const string ITEMS = "Scriptables/Items";
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
