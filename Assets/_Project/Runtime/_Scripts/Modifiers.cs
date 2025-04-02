#region
using UnityEngine;
#endregion

public class Modifiers : MonoBehaviour
{
	[SerializeField] float speed = 1;
	[SerializeField] float damage = 1;

	public float Speed => speed;
	public float Damage => damage;

	public void SetModifiers(string modifier, float value)
	{
		switch (modifier)
		{
			case "Speed":
				speed = value;
				break;

			case "Damage":
				damage = value;
				break;

			default:
				Debug.LogWarning($"Modifier {modifier} not found.");
				break;
		}
	}

	public void Add(string modifier, float value)
	{
		switch (modifier)
		{
			case "Speed":
				speed += value;
				break;

			case "Damage":
				damage += value;
				break;

			default:
				Debug.LogWarning($"Modifier {modifier} not found.");
				break;
		}
	}

	public void Remove(string modifier, float value)
	{
		switch (modifier)
		{
			case "Speed":
				speed -= value;
				break;

			case "Damage":
				damage -= value;
				break;

			default:
				Debug.LogWarning($"Modifier {modifier} not found.");
				break;
		}
	}
}
