/*
 * All rights to the Sounds Good plugin, © Created by Melenitas Dev, are reserved.
 * Distribution of the standalone asset is strictly prohibited.
 */
#region
using System;
using UnityEngine;
using Random = UnityEngine.Random;
#endregion

namespace MelenitasDev.SoundsGood.Domain
{
public partial class SoundData // Serialized Fields
{
	[SerializeField] string tag;
	[SerializeField] AudioClip[] clips;
	[SerializeField] CompressionPreset compressionPreset;
	[SerializeField] bool forceToMono;
}

public partial class SoundData // Properties
{
	public string Tag
	{
		get => tag;
		set => tag = value;
	}
	public AudioClip[] Clips
	{
		get => clips;
		set => clips = value;
	}
	public CompressionPreset CompressionPreset
	{
		get => compressionPreset;
		set => compressionPreset = value;
	}
	public bool ForceToMono
	{
		get => forceToMono;
		set => forceToMono = value;
	}
}

[Serializable]
public partial class SoundData // Public Methods
{
	public SoundData(string tag, AudioClip[] clips, CompressionPreset compressionPreset, bool forceToMono)
	{
		this.tag = tag;
		this.clips = clips;
		this.compressionPreset = compressionPreset;
		this.forceToMono = forceToMono;
	}

	public AudioClip GetClip() => clips[Random.Range(0, clips.Length)];
}
}
