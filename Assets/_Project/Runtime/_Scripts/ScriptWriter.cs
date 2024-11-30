#region
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
#endregion

public static class ScriptWriter
{
	// Directory where the generated .cs scripts will be saved
	const string savePath = "Assets/_Project/Runtime/_Scripts/Status Effects/";

	static bool isWriting;

	// Function to create the .cs script for a given buff
	public static void WriteScript(string name, string description, string type, string duration, string target, string appliesTiming)
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

		// Description is already formatted :)
		scriptContent.Replace("$DESCRIPTION", description);

		// strip the seconds from the duration
		duration = duration.Contains(" seconds") ? duration.Replace(" seconds", string.Empty) : duration;
		scriptContent.Replace("$DURATION", duration);

		// Prepend the target with "TargetType." to match the enum in the base class
		target = "TargetType." + target;
		scriptContent.Replace("$TARGET", target);

		// Convert the appliesTiming to a boolean
		appliesTiming = appliesTiming == "Early" ? "false" : "true";
		scriptContent.Replace("$APPLIES_TIMING", appliesTiming);

		// Ensure the directory exists
		if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);

		// Define the script file path
		string filePath = Path.Combine(savePath, name + ".cs");

		// Write the script to the specified file path
		File.WriteAllText(filePath, scriptContent.ToString());

		// Refresh the AssetDatabase to ensure the new scripts show up in the editor
		AssetDatabase.ImportAsset(filePath);
	}
}
