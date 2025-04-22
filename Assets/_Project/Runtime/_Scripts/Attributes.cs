#region
using UnityEngine;
#endregion

public class Attributes : MonoBehaviour
{
	[Header("Modifiers")]
	[SerializeField] float speed = 1;
	[SerializeField] float damage = 1;
	[Tooltip("A modifier that affects the cast time of spells.")]
	[SerializeField] float castSpeed = 1;
	[Tooltip("A modifier that affects the cooldown of spells.")]
	[SerializeField] float spellSpeed = 1;
	
	[Header("Attributes")]
	[Tooltip("A shield prevents a single instance of damage from being applied.")]
	[SerializeField] int shields;

	public float Speed => speed;
	public float Damage => damage;
	public float CastSpeed => castSpeed;
	public float SpellSpeed => spellSpeed;
	public int Shields => shields;
	
	public enum Stats
	{
		Speed,
		Damage,
		CastSpeed,
		SpellSpeed,
		Shields
	}

	void Validate()
	{
		Debug.Assert(speed > 0, "Speed cannot be negative or 0.");
		Debug.Assert(damage > 0, "Damage cannot be negative or 0.");
		Debug.Assert(castSpeed >= 0, "Cast speed cannot be negative.");
		Debug.Assert(spellSpeed > 0, "Spell speed cannot be negative or 0.");
		Debug.Assert(shields >= 0, "Shields cannot be negative.");
	}

	public void Add(Stats stat, float value)
	{
		switch (stat)
		{
			// modifiers
			
			case Stats.Speed:
				speed += Mathf.Clamp(value, -byte.MaxValue, byte.MaxValue);
				break;

			case Stats.Damage:
				damage += Mathf.Clamp(value, -byte.MaxValue, byte.MaxValue);
				break;

			case Stats.CastSpeed:
				castSpeed += Mathf.Clamp(value, -byte.MaxValue, byte.MaxValue);
				break;

			case Stats.SpellSpeed:
				spellSpeed += Mathf.Clamp(value, -byte.MaxValue, byte.MaxValue);
				break;
			
			// attributes

			case Stats.Shields:
				shields += Mathf.Clamp((int) value, 0, byte.MaxValue);
				break;

			default:
				Debug.LogWarning($"Modifier {stat} not found.");
				break;
		}
		
		Validate();
	}

	public void Remove(Stats attribute, float value)
	{
		switch (attribute)
		{
			// modifiers
			
			case Stats.Speed:
				speed -= Mathf.Clamp(value, -byte.MaxValue, byte.MaxValue);
				break;
	
			case Stats.Damage:
				damage -= Mathf.Clamp(value, -byte.MaxValue, byte.MaxValue);
				break;
	
			case Stats.CastSpeed:
				castSpeed -= Mathf.Clamp(value, -byte.MaxValue, byte.MaxValue);
				break;
	
			case Stats.SpellSpeed:
				spellSpeed -= Mathf.Clamp(value, -byte.MaxValue, byte.MaxValue);
				break;
			
			// attributes
	
			case Stats.Shields:
				shields -= Mathf.Clamp((int) value, 0, byte.MaxValue);
				break;
	
			default:
				Debug.LogWarning($"Attribute {attribute} not found.");
				break;
		}
		
		Validate();
	}
}
