#region
using System.Collections;
using UnityEngine;
#endregion

public class Zetaflare : Debuff
{
	public override void Reset()
	{
		statusName = "Zetaflare";
		description = "Bahamut gathers all the ambient aether over the course of 10 seconds. Upon fully charging up, he strikes the target, killing them instantly.";
		duration = 10;
		target = Target.Enemy;
		timing = Timing.Prefix;
	}

	protected override void OnInvoke()
	{
		entity.StartCoroutine(Wait());

		return;

		IEnumerator Wait()
		{
			yield return new WaitForSeconds(duration);
			entity.TakeDamage(9999);
		}
	}
}
