#region
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
#endregion

/// <summary>
/// Performs actions before the build process starts.
/// </summary>
public class PreprocessBuild : IPreprocessBuildWithReport
{
    public int callbackOrder { get; }
    
    public void OnPreprocessBuild(BuildReport report)
    {
        Logger.LogWarning("Preprocessing the build...");
        
        // do something
    }
}