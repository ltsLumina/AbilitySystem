#if UNITY_EDITOR
#region
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
#endregion

public static class ScriptWriter
{
	// Directory where the generated .cs scripts will be saved
	const string savePath = AbilitySettings.FullPaths.STATUS_EFFECTS_CS;

	// Function to create or update the .cs script for a given buff
	public static void WriteScript(string name, string description, string type, string duration, string target, string timing)
	{
		// Load the template for the Reset() method
		string resetMethodTemplate = @"
    public override void Reset()
    {
        statusName = ""$NICE_NAME"";
        description = ""$DESCRIPTION"";
        duration = $DURATION;
        target = $TARGET;
        timing = $TIMING;
    }";

		// Replace placeholders in the Reset() template
		name = name.Replace(" ", string.Empty); // Ensure valid class name
		description = description.Replace(";", ",");
		duration = duration.Contains(" seconds") ? duration.Replace(" seconds", string.Empty) : duration;

		// replace quotes with empty string
		target = target.Replace("\"", string.Empty);

		timing = timing.Contains("Pre") ? "Timing.Prefix" : "Timing.Postfix";

		resetMethodTemplate = resetMethodTemplate.Replace("$NICE_NAME", ObjectNames.NicifyVariableName(name)).Replace("$DESCRIPTION", description).Replace("$DURATION", duration).Replace("$TARGET", target).Replace("$TIMING", timing);

		// Ensure the directory exists
		if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);

		// Define the script file path
		string filePath = Path.Combine(savePath, name + ".cs");

		string scriptContent;

		if (File.Exists(filePath))
		{
			// Read the existing script
			scriptContent = File.ReadAllText(filePath);

			// Replace only the Reset() method
			int startIndex = scriptContent.IndexOf("protected override void Reset()", StringComparison.Ordinal);

			if (startIndex >= 0)
			{
				int endIndex = scriptContent.IndexOf("}", startIndex, StringComparison.Ordinal) + 1; // Find the closing brace of Reset()
				scriptContent = scriptContent.Remove(startIndex, endIndex - startIndex).Insert(startIndex, resetMethodTemplate);
			}
		}
		else
		{
			// Create a new script if it doesn't exist
			string classTemplate = $@"
using UnityEngine;

public class {name} : {type}
{{
    {resetMethodTemplate}
}}";

			scriptContent = classTemplate;
		}

		// Write the updated script to the file
		File.WriteAllText(filePath, scriptContent);

		// Refresh the AssetDatabase to ensure the new scripts show up in the editor
		AssetDatabase.ImportAsset(filePath);
		AssetDatabase.Refresh();
	}

	public class StatusEffectAssetPostprocessor : AssetPostprocessor
	{
		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			// Create the ScriptableObject instance for each imported script
			foreach (string assetPath in importedAssets)
			{
				// Only proceed if the imported asset is in the "_Scripts/Status Effects" directory
				if (!assetPath.Contains("_Scripts/Status Effects")) continue;

				if (assetPath.Contains(".cs"))
				{
					string name = Path.GetFileNameWithoutExtension(assetPath);
					CreateScriptableObject(name);
				}
			}
		}

		static void CreateScriptableObject(string name)
		{
			string formattedName = ObjectNames.NicifyVariableName(name);

			// Define the path where the ScriptableObject asset should be saved
			string assetPath = $"Assets/_Project/Runtime/Resources/Scriptables/Status Effects/{name}.asset";
			string checkPath = $"Assets/_Project/Runtime/Resources/Scriptables/Status Effects/{formattedName}.asset";

			// Check if the asset already exists by name
			var existingAsset = AssetDatabase.LoadAssetAtPath<StatusEffect>(checkPath);

			if (existingAsset != null)
			{
				existingAsset.Reset(); // Update the values in the inspector.
				Logger.Log($"The StatusEffect \"{name}\" already exists at [{assetPath}]. Skipping creation.");
				return;
			}

			// Get the type of the ScriptableObject by reflection after the script has been compiled
			var scriptType = Type.GetType(name);

			if (scriptType == null)
			{
				Logger.LogError($"Failed to find type for {name}. Make sure the script compiles correctly.");
				return;
			}

			// Create an instance of the ScriptableObject
			var instance = ScriptableObject.CreateInstance(scriptType);

			// Save the ScriptableObject asset to disk
			AssetDatabase.CreateAsset(instance, assetPath);
			AssetDatabase.RenameAsset(assetPath, ObjectNames.NicifyVariableName(name));
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
	}
}

#endif
