#region
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;
using Lumina.Essentials.Attributes;
using UnityEditor;
using UnityEngine;
#endregion

/// <summary>
///     The job (or class) of a player character.
///     <para>Used to determine the abilities a player can use, as well as their stats.</para>
///     Also used to determine the player's character model, animations, and other visual elements.
/// </summary>
[CreateAssetMenu(fileName = "New Job", menuName = "Jobs/Job")]
public sealed class Job : ScriptableObject
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum Class
    {
        RPR,
        RDM,
        DRK,
        SGE,

        DEV, // Special class for developers
    }

    [SerializeField] Class job;
    [SerializeField] [ReadOnly] List<Ability> abilities = new ();

    public List<Ability> Abilities
    {
        get
        {
            if (abilities.Count != 4) Logger.LogError($"Not enough abilities for {job}.");
            return abilities;
        }
    }

    void OnValidate()
    {
        Ability[] resources = Resources.LoadAll<Ability>(AbilitySettings.ResourcesPath);
        abilities = resources.Where(a => a.job == job).ToList();

        PlayModePreventer.preventPlayMode = abilities.Count != 4;

        switch (abilities.Count)
        {
            case 0:
                Logger.LogError($"No abilities found for {job}.");
                break;

            case var count when count != 4:
                Logger.LogError($"Not enough abilities for {job}. There must be exactly 4 abilities.");
                break;
        }
    }
}

[InitializeOnLoad]
public static class PlayModePreventer
{
    [UsedImplicitly]
    public static bool preventPlayMode { get; set; }

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
