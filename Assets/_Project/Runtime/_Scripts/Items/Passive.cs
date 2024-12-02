#region
using UnityEngine;
#endregion

[CreateAssetMenu(fileName = "New Passive Item", menuName = "Items/New Passive Item", order = 1)]
public class Passive : ItemAttributes
{
	Player player => FindAnyObjectByType<Player>();

	protected override void OnInvoke()
	{
		Debug.Log("yay it did a thing");

		PhoenixCharm();
	}

	void PhoenixCharm()
	{
		// player.StartCoroutine(Wait());
		//
		// return;
		// IEnumerator Wait()
		// {
		// 	yield return new WaitUntil(() => player.health < player.maxHealth);
		// 	player.health = player.maxHealth;
		// 	Logger.Log("Reviving player!");
		// }
	}
}
