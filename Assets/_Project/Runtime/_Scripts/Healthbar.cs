#region
using JetBrains.Annotations;
using Lumina.Essentials.Attributes;
using UnityEngine;
#endregion

public class Healthbar : MonoBehaviour
{
	[UsedImplicitly, ReadOnly]
	[SerializeField] int hearts;
	[ReadOnly]
	[SerializeField] Player owner;
	
	Transform[] heartIcons;

	void Start()
	{
		// Store heart icons for better performance
		heartIcons = new Transform[transform.childCount];
		for (int i = 0; i < transform.childCount; i++) heartIcons[i] = transform.GetChild(i);

		// Subscribe to player join event
		PlayerManager.Instance.OnPlayerJoined += InitializeHealthbar;
	}

	void InitializeHealthbar(Player player)
	{
		Transform parent = transform.parent;

		if (!player.CompareTag(parent.tag)) return;
		
		owner = player;
		hearts = owner.MaxHealth;
		owner.OnTookDamage += UpdateHealthDisplay;

		// Initial health display
		UpdateHealthDisplay(false);
	}

	void UpdateHealthDisplay(bool hadShields)
	{
		if (owner == null) return;

		for (int i = 0; i < heartIcons.Length; i++)
		{
			heartIcons[i].gameObject.SetActive(i < owner.Health);
			hearts = owner.Health;
		}
	}
}
