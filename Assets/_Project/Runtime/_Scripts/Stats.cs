#region
using UnityEngine;
#endregion

public class Stats : MonoBehaviour
{
	[SerializeField] float speed = 1;
	[SerializeField] float damage = 1;
	[Tooltip("A shield prevents a single instance of damage from being applied.")]
	[SerializeField] int shields;
	[Tooltip("A modifier that affects the cast time of spells.")]
	[SerializeField] float castSpeed = 1;
	[Tooltip("A modifier that affects the cooldown of spells.")]
	[SerializeField] float spellSpeed = 1;

	public float Speed => speed;
	public float Damage => damage;
	public int Shields => shields;
	public float CastSpeed => castSpeed;
	public float SpellSpeed => spellSpeed;

	public void SetModifier(string modifier, float value)
	{
		switch (modifier.ToLowerInvariant())
		{
			case "speed":
				speed = Mathf.Clamp(value, -byte.MaxValue, byte.MaxValue);
				break;

			case "damage":
				damage = Mathf.Clamp(value, -byte.MaxValue, byte.MaxValue);
				break;

			case "shields":
				shields = Mathf.Clamp((int) value, 0, byte.MaxValue);
				break;

			case "cast speed":
				castSpeed = Mathf.Clamp(value, -byte.MaxValue, byte.MaxValue);
				break;

			case "spell speed":
				spellSpeed = Mathf.Clamp(value, -byte.MaxValue, byte.MaxValue);
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
				speed += Mathf.Clamp(value, -byte.MaxValue, byte.MaxValue);
				break;

			case "damage":
				damage += Mathf.Clamp(value, -byte.MaxValue, byte.MaxValue);
				break;

			case "shields":
				shields += Mathf.Clamp((int) value, 0, byte.MaxValue);
				break;

			case "cast speed":
				castSpeed += Mathf.Clamp(value, -byte.MaxValue, byte.MaxValue);
				break;

			case "spell speed":
				spellSpeed += Mathf.Clamp(value, -byte.MaxValue, byte.MaxValue);
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
				speed -= Mathf.Clamp(value, -byte.MaxValue, byte.MaxValue);
				break;

			case "damage":
				damage -= Mathf.Clamp(value, -byte.MaxValue, byte.MaxValue);
				break;

			case "shields":
				shields -= Mathf.Clamp((int) value, 0, byte.MaxValue);
				break;

			case "cast speed":
				castSpeed -= Mathf.Clamp(value, -byte.MaxValue, byte.MaxValue);
				break;

			case "spell speed":
				spellSpeed -= Mathf.Clamp(value, -byte.MaxValue, byte.MaxValue);
				break;

			default:
				Debug.LogWarning($"Modifier {modifier} not found.");
				break;
		}
	}
}
