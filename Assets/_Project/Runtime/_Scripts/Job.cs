#region
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Lumina.Essentials.Attributes;
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

        if (abilities.Count == 0) Debug.LogWarning($"No abilities found for {job}.");
        Debug.Assert(abilities.Count == 4, $"Not enough abilities for {job}. There must be exactly 4 abilities.");
    }
}
