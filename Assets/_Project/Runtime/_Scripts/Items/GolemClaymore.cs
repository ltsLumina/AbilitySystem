#region
using UnityEngine;
#endregion

[CreateAssetMenu(fileName = "New Active Item", menuName = "Items/New Active Item", order = 0)]
public class Active : ItemAttributes
{
	protected override void OnInvoke()
	{
		Debug.Log("yay it did a thing (active)");

		GolemClaymore();
	}

	void GolemClaymore()
	{
		// player.StartCoroutine(Wait());
		//
		// return;
		// IEnumerator Wait()
		// {
		// 	yield return new WaitUntil(() => condition);
		// 	
		// }
	}
}
