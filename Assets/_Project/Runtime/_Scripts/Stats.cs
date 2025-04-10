#region
using UnityEngine;
#endregion

public class Stats : MonoBehaviour
{
	[SerializeField] float speed = 1;
	[SerializeField] float damage = 1;
	[Tooltip("A shield prevents a single instance of damage from being applied.")]
	[SerializeField] int shields;

	public float Speed => speed;
	public float Damage => damage;
	public int Shields => shields;

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

			case "shields":
				shields = (int) value;
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

			case "shields":
				shields += (int) value;
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

			case "shields":
				shields -= (int) value;
				break;

			default:
				Debug.LogWarning($"Modifier {modifier} not found.");
				break;
		}
	}
}
