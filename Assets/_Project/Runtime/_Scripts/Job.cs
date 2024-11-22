#region
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    }

    [SerializeField] Class job;
    [SerializeField] List<Ability> abilities = new ();

    public List<Ability> Abilities => abilities;
}
