#region
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
#endregion

public class AttackEnumGenerator
{
	const string MENU_PATH = "Tools/Generate Attack Enum %#g"; // Ctrl+Shift+G shortcut
	const string ENUM_FILE_PATH = "Assets/_Project/Runtime/_Scripts/Boss/Attacks.cs";

	[MenuItem(MENU_PATH)]
	public static void GenerateEnum()
	{
		// Get current enum values if they exist
		var existingAttacks = new HashSet<string>();

		if (File.Exists(ENUM_FILE_PATH))
		{
			string[] lines = File.ReadAllLines(ENUM_FILE_PATH);

			foreach (string line in lines)
			{
				string trimmedLine = line.Trim();
				if (!string.IsNullOrEmpty(trimmedLine) && !trimmedLine.StartsWith("//") && !trimmedLine.StartsWith("{") && !trimmedLine.StartsWith("}") && !trimmedLine.StartsWith("public enum")) existingAttacks.Add(trimmedLine.TrimEnd(',', ' '));
			}
		}

		// Get current methods from AttackData
		Type type = typeof(AttackData);
		IEnumerable<MethodInfo> methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(m => m.GetParameters().Length == 3);

		string[] exclude =
		{ "InvokeRepeating", "SendMessageUpwards", "SendMessage", "BroadcastMessage" };

		// Sort methods by their MetadataToken to preserve source code order
		List<string> currentMethods = methods.Where(m => !exclude.Contains(m.Name)).OrderBy(m => m.MetadataToken).Select(m => m.Name).ToList();

		var currentMethodsSet = new HashSet<string>(currentMethods);

		// Check if there are any changes
		if (!existingAttacks.SetEquals(currentMethodsSet))
		{
			List<string> added = currentMethodsSet.Except(existingAttacks).ToList();
			List<string> removed = existingAttacks.Except(currentMethodsSet).ToList();

			// Generate the updated enum file
			using (var writer = new StreamWriter(ENUM_FILE_PATH))
			{
				writer.WriteLine("// This file is auto-generated. Do not edit it.");
				writer.WriteLine("public enum Attacks");
				writer.WriteLine("{");

				// Write methods in their original order
				foreach (string methodName in currentMethods) writer.WriteLine($"\t{methodName},");

				writer.WriteLine("}");
			}

			AssetDatabase.ImportAsset(ENUM_FILE_PATH);

			// Log changes
			if (added.Any()) Debug.Log($"Added attacks: {string.Join(", ", added)}");
			if (removed.Any()) Debug.Log($"Removed attacks: {string.Join(", ", removed)}");

			Debug.Log("Attack enum updated successfully!");
		}
		else Debug.Log("Attack enum is already up to date.");
	}

	// Validate the menu item
	[MenuItem(MENU_PATH, true)]
	static bool ValidateGenerate() => typeof(AttackData) != null;
}
