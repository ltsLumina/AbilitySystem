#region
using UnityEngine;
#endregion

public class Modifiers : MonoBehaviour
{
	[SerializeField] float speed = 1;
	[SerializeField] float damage = 1;

	public float Speed => speed;
	public float Damage => damage;

	public void SetModifier(string modifier, float value)
	{
		switch (modifier.ToLowerInvariant())
		{
			case "speed":
				speed = value;
				break;

			case "damage":
				damage = value;
				break;

			default:
				Debug.LogWarning($"Modifier {modifier} not found.");
				break;
		}
	}

	public void Add(string modifier, float value)
	{
		switch (modifier.ToLowerInvariant())
		{
			case "speed":
				speed += value;
				break;

			case "damage":
				damage += value;
				break;

			default:
				Debug.LogWarning($"Modifier {modifier} not found.");
				break;
		}
	}

	public void Remove(string modifier, float value)
	{
		switch (modifier.ToLowerInvariant())
		{
			case "speed":
				speed -= value;
				break;

			case "damage":
				damage -= value;
				break;

			default:
				Debug.LogWarning($"Modifier {modifier} not found.");
				break;
		}
	}
}
