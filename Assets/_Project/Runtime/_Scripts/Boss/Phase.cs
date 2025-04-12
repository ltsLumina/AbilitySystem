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
	[SerializeField] int startIndex;
	[SerializeField] List<Behaviour> behaviours = new ();

	Entity self;

	public event Action<Phase> OnPhaseEnded;

	// ReSharper disable once ParameterHidesMember
	public void Start(Entity self)
	{
		this.self = self;
		this.self.StartCoroutine(Wait());
	}

	public void Stop() => self.StopAllCoroutines();

	IEnumerator Wait()
	{
		for (int i = startIndex; i < behaviours.Count; i++)
		{
			behaviours[i].Start(self);
			yield return new WaitForSeconds(behaviours[i].Duration);
		}

		End();
	}

	void End()
	{
		Debug.Log($"Phase \"{name}\" ended.");
		OnPhaseEnded?.Invoke(this);
	}
}
