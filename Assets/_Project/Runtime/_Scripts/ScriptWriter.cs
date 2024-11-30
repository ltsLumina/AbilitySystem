#region
using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
#endregion

public static class ScriptWriter
{
	// Directory where the generated .cs scripts will be saved
	const string savePath = "Assets/_Project/Runtime/_Scripts/Status Effects/";

	// Function to create the .cs script for a given buff
	public static void WriteScript(string name, string description, string type, string duration, string target, string timing)
	{
		// Experimental Script template
		string scriptTemplateFile = Resources.Load<TextAsset>("TemplateStatusEffect").text;

		// Use StringBuilder for efficient string manipulation
		StringBuilder scriptContent = new (scriptTemplateFile);

		// Perform replacements in a specific order

		// Make name a valid class name (no spaces)
		name = name.Replace(" ", string.Empty);
		scriptContent.Replace("$NAME", name);

		// Nicify name for the statusName field
		scriptContent.Replace("$NICE_NAME", ObjectNames.NicifyVariableName(name));

		// Prepend the type with "StatusEffect" to include the base class
		type = "StatusEffect." + type;
		scriptContent.Replace("$TYPE", type);

		// Replace semicolons with commas in the description (semicolons are used instead of commas in the CSV)
		description = description.Replace(";", ",");
		scriptContent.Replace("$DESCRIPTION", description);

		// strip the seconds from the duration
		duration = duration.Contains(" seconds") ? duration.Replace(" seconds", string.Empty) : duration;
		scriptContent.Replace("$DURATION", duration);

		// Prepend the target with "TargetType." to match the enum in the base class
		target = "Target." + target;
		scriptContent.Replace("$TARGET", target);

		// Convert the timing to the enum value (check if timing contains "Pre" or "Post")
		timing = timing.Contains("Pre") ? "Timing.Prefix" : "Timing.Postfix";
		scriptContent.Replace("$TIMING", timing);

		// Ensure the directory exists
		if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);

		// Define the script file path
		string filePath = Path.Combine(savePath, name + ".cs");

		// Write the script to the specified file path
		File.WriteAllText(filePath, scriptContent.ToString());

		// Refresh the AssetDatabase to ensure the new scripts show up in the editor
		AssetDatabase.ImportAsset(filePath);
		AssetDatabase.Refresh();

		// After importing, create the ScriptableObject instance
		CreateScriptableObject(name);
	}

	static void CreateScriptableObject(string name)
	{
		// Get the type of the ScriptableObject by reflection after the script has been compiled
		var scriptType = Type.GetType(name);

		if (scriptType == null)
		{
			Debug.LogError($"Failed to find type for {name}. Make sure the script compiles correctly.");
			return;
		}

		// Create an instance of the ScriptableObject
		var instance = ScriptableObject.CreateInstance(scriptType);

		// Define where to save the ScriptableObject asset
		string assetPath = $"Assets/_Project/Runtime/Resources/Scriptables/Status Effects/{name}.asset";

		// Save the ScriptableObject asset to disk
		AssetDatabase.CreateAsset(instance, assetPath);
		AssetDatabase.RenameAsset(assetPath, ObjectNames.NicifyVariableName(name));
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}
}
