#region
using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
#endregion

[Serializable]
public class Phase
{
	[UsedImplicitly]
	[HideInInspector] public string name;
	[SerializeField] List<Behaviour> behaviours = new ();

	Entity context;

	// ReSharper disable once ParameterHidesMember
	public void Start(Entity context)
	{
		this.context = context;
		this.context.StartCoroutine(Wait());
	}

	IEnumerator Wait()
	{
		behaviours[0].Start(context);
		yield return new WaitForSeconds(3f);
		behaviours[1].Start(context);
		yield return new WaitForSeconds(3f);
		behaviours[2].Start(context);
		yield return new WaitForSeconds(3f);
	}
}
