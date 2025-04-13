/*
 * All rights to the Sounds Good plugin, © Created by Melenitas Dev, are reserved.
 * Distribution of the standalone asset is strictly prohibited.
 */
#region
using System;
using UnityEngine;
using UnityEngine.Audio;
#endregion

namespace MelenitasDev.SoundsGood.Domain
{
public partial class OutputData // Serialized Fields
{
	[SerializeField] string name;
	[SerializeField] AudioMixerGroup output;
}

public partial class OutputData // Properties
{
	public string Name
	{
		get => name;
		set => name = value;
	}
	public AudioMixerGroup Output
	{
		get => output;
		set => output = value;
	}
}

[Serializable]
public partial class OutputData // Public Methods
{
	public OutputData(string name, AudioMixerGroup output)
	{
		this.name = name;
		this.output = output;
	}
}
}
