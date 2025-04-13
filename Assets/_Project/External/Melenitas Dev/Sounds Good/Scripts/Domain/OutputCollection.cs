/*
 * All rights to the Sounds Good plugin, © Created by Melenitas Dev, are reserved.
 * Distribution of the standalone asset is strictly prohibited.
 */
#region
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
#endregion

namespace MelenitasDev.SoundsGood.Domain
{
public class OutputCollection : ScriptableObject
{
	[SerializeField] OutputData[] outputs = Array.Empty<OutputData>();

	Dictionary<string, OutputData> outputsDictionary = new ();

	public OutputData[] Outputs => outputs;

	public void Init()
	{
		foreach (OutputData outputData in outputs) outputsDictionary.Add(outputData.Name, outputData);
	}

	public void LoadOutputs()
	{
		var mixer = Resources.Load<AudioMixer>("Melenitas Dev/Sounds Good/Outputs/Master");
		AudioMixerGroup[] mixerGroups = mixer.FindMatchingGroups(null);
		var loadedOutputs = new OutputData[mixerGroups.Length];

		for (int i = 0; i < loadedOutputs.Length; i++)
		{
			var newOutputData = new OutputData(mixerGroups[i].name.Replace(" ", ""), mixerGroups[i]);
			loadedOutputs[i] = newOutputData;
			Debug.Log($"Output {i} '{newOutputData.Name}' saved!");
		}

		outputs = loadedOutputs;
	}

	public AudioMixerGroup GetOutput(string name)
	{
		if (outputsDictionary.TryGetValue(name.Replace(" ", ""), out OutputData outputData)) return outputData.Output;

		Debug.LogWarning($"Output with tag '{name}' don't exist");
		return null;
	}
}
}
