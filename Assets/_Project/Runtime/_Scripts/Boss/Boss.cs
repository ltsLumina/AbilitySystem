#region
using System.Collections.Generic;
using UnityEngine;
using VInspector;
#endregion

public class Boss : Entity
{
	[HideInInspector]
	public VInspectorData data;

	[Foldout("Sequence")]
	[SerializeField] List<Phase> phases = new ();
	[SerializeField] int currentBehaviourIndex;

	protected override void OnStart()
	{
		Debug.Log("Boss started.");
		phases[0].Start(this);
	}
}
