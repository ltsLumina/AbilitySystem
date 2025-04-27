#region
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
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
        // Dictionary to store existing enum values and their numeric values
        var existingAttacks = new Dictionary<string, int>();

        if (File.Exists(ENUM_FILE_PATH))
        {
            string[] lines = File.ReadAllLines(ENUM_FILE_PATH);
            var valueRegex = new Regex(@"^\s*(\w+)\s*=\s*(\d+),?\s*$");

            foreach (string line in lines)
            {
                var match = valueRegex.Match(line);
                if (match.Success)
                {
                    string enumName = match.Groups[1].Value;
                    int enumValue = int.Parse(match.Groups[2].Value);
                    existingAttacks[enumName] = enumValue;
                }
            }
        }

        // Get current methods from AttackData
        Type type = typeof(AttackData);
        IEnumerable<MethodInfo> methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetParameters().Length == 3);

        string[] exclude = { "InvokeRepeating", "SendMessageUpwards", "SendMessage", "BroadcastMessage" };

        // Sort methods by their MetadataToken to preserve source code order
        List<string> currentMethods = methods
            .Where(m => !exclude.Contains(m.Name))
            .OrderBy(m => m.MetadataToken)
            .Select(m => m.Name)
            .ToList();

        // Find next available value for new entries
        int nextValue = existingAttacks.Count > 0 ? existingAttacks.Values.Max() + 1 : 0;

        // Check if there are any changes
        bool hasChanges = currentMethods.Any(m => !existingAttacks.ContainsKey(m)) || 
                         existingAttacks.Keys.Any(k => !currentMethods.Contains(k));

        if (hasChanges)
        {
            List<string> added = currentMethods.Where(m => !existingAttacks.ContainsKey(m)).ToList();
            List<string> removed = existingAttacks.Keys.Where(k => !currentMethods.Contains(k)).ToList();

            // Generate the updated enum file
            using (var writer = new StreamWriter(ENUM_FILE_PATH))
            {
                writer.WriteLine("// This file is auto-generated. Do not edit it.");
                writer.WriteLine("public enum Attacks");
                writer.WriteLine("{");

                // Write methods with their values, preserving existing values and assigning new ones
                foreach (string methodName in currentMethods)
                {
                    int value = existingAttacks.TryGetValue(methodName, out int existingValue) 
                        ? existingValue 
                        : nextValue++;
                    
                    writer.WriteLine($"\t{methodName} = {value},");
                }

                writer.WriteLine("}");
            }

            AssetDatabase.ImportAsset(ENUM_FILE_PATH);

            // Log changes
            if (added.Any()) Debug.Log($"Added attacks: {string.Join(", ", added)}");
            if (removed.Any()) Debug.Log($"Removed attacks: {string.Join(", ", removed)}");

            Debug.Log("Attack enum updated successfully!");
        }
        else
        {
            Debug.Log("Attack enum is already up to date.");
        }
    }

    // Validate the menu item
    [MenuItem(MENU_PATH, true)]
    static bool ValidateGenerate() => typeof(AttackData) != null;
}