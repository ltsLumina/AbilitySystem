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

		DEV, // Special developer class
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
		Ability[] resources = Resources.LoadAll<Ability>(AbilitySettings.ResourcePaths.ABILITIES);
		abilities = resources.Where(a => a.Job == job).ToList();
		if (resources.Length == 0) return;

		//PlayModePreventer.preventPlayMode = abilities.Count != 4;
		//PlayModePreventer.Reason($"Not enough abilities for {job}. There must be exactly 4 abilities." + "\n" + "The following abilities were found: " + string.Join(", ", abilities.Select(a => a.name) + $"(Count: {abilities.Count})"), this);

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
