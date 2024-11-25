using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class PlayModePreventer
{
    [UsedImplicitly]
    public static bool preventPlayMode { get; set; }

    public static void Reason(string reason, Object context = null)
    {
        if (!preventPlayMode) return;
        Logger.LogError(reason, context);
        EditorApplication.isPlaying = false;
    }
    
    static PlayModePreventer() { EditorApplication.playModeStateChanged += OnPlayModeStateChanged; }

    static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            if (!preventPlayMode) return;

            List<Job> missingAbilities = Object.FindObjectsByType<Job>(FindObjectsSortMode.None).Where(j => j.Abilities.Count != 4).ToList();
            string jobs = string.Join(", ", missingAbilities.Select(j => j.name));

            Logger.LogError("Cannot enter playmode when there are missing abilities. Please ensure that each job has exactly 4 abilities." + "\n" + $"The following jobs have missing abilities: {jobs}");
            EditorApplication.isPlaying = false;
        }
    }
}
