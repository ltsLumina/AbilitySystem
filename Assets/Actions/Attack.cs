#region
using System;
using TheKiwiCoder;
#endregion

[Serializable]
public class Attack : ActionNode
{
	protected override void OnStart() => context.animator.SetTrigger("Rotate");

	protected override void OnStop() { }

	protected override State OnUpdate() => State.Success;
}
