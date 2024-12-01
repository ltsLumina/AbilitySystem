#region
using System;
using TheKiwiCoder;
using UnityEngine;
#endregion

[Serializable]
public class Move : ActionNode
{
	protected override void OnStart() =>

			// set pos to -5 if its 5, and vice versa
			context.transform.position = context.transform.position == new Vector3(5, 0, 0) ? new (-5, 0, 0) : new Vector3(5, 0, 0);

	protected override void OnStop() { }

	protected override State OnUpdate() => State.Success;
}
