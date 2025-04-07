#region
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#endregion

[Serializable]
public class Phase
{
	[SerializeField] string name;
	[SerializeField] List<Behaviour> behaviours = new ();

	Entity self;

	public event Action<Phase> OnPhaseEnded;

	// ReSharper disable once ParameterHidesMember
	public void Start(Entity self)
	{
		this.self = self;
		this.self.StartCoroutine(Wait());
	}

	IEnumerator Wait()
	{
		foreach (Behaviour behaviour in behaviours)
		{
			behaviour.Start(self);
			yield return new WaitForSeconds(behaviour.Duration);
		}

		End();
	}

	void End()
	{
		Debug.Log($"Phase \"{name}\" ended.");
		OnPhaseEnded?.Invoke(this);
	}
}
