#region
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
#endregion

public static class EditorGUIUtils
{
    public readonly static GUIContent createdSuccessfullyContent = new ("Created Successfully.");
    public readonly static GUIContent nameContent = new ("Name", "The name of the move. This will be the name of the ScriptableObject as well.");
    public readonly static GUIContent damageContent = new ("Damage", "The damage value of the move.");
    public readonly static GUIContent descriptionContent = new ("Description", "The description of the move.");
    public readonly static GUIContent maxHealthContent = new ("Max Health", "The maximum health of the enemy.");
    public readonly static GUIContent speedContent = new ("Speed", "The speed of the enemy.");
    public readonly static GUIContent xpYieldContent = new ("XP Yield", "The amount of experience the enemy yields when defeated.");

    /// <summary>
    ///     Creates a new script file based on a template.
    /// </summary>
    /// <param name="templateType">The type of the script to create (e.g., Item, Enemy).</param>
    /// <param name="directory">The directory where the script will be saved.</param>
    /// <param name="className">The class name used in the template file.</param>
    /// <param name="templatePath">The path to the template file.</param>
    /// <param name="name">The name of the new script file and class.</param>
    /// <exception cref="System.Exception">Thrown when the script creation fails.</exception>
    internal static Type CreateScript(Type templateType, string directory, string className, string templatePath, string name)
    {
        string assetPath = EditorUtility.SaveFilePanel("Save Item", directory, name, "cs");

        if (string.IsNullOrEmpty(assetPath))
        {
            EditorUtility.DisplayDialog("Script creation aborted", "Cancel button pressed.\nAborting script creation.", "OK");
            return null;
        }

        try
        {
            // Read the template file
            string templateContent = File.ReadAllText(templatePath);

            // Write the template content to the new script file
            File.WriteAllText(assetPath, templateContent);

            // Replace the class name in the template with the name of the new script
            string scriptContent = File.ReadAllText(assetPath);
            scriptContent = scriptContent.Replace(className, name).Replace($"{templateType}TemplateFile", name);

            File.WriteAllText(assetPath, scriptContent);
        } catch (Exception e)
        {
            Debug.LogError($"Failed to create script: {e.Message}");
            throw;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Load the newly created type
        string scriptAssetPath = "Assets" + assetPath[Application.dataPath.Length..];
        var    monoScript      = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptAssetPath);

        if (!monoScript)
        {
            Debug.LogError("Failed to load the newly created script.");
            return null;
        }

        return monoScript.GetClass();
    }
}
