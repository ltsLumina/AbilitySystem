#region
using System.Diagnostics.CodeAnalysis;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;
#endregion

public class Item : MonoBehaviour
{
	public enum Rarity
	{
		Ruby,
		Garnet,
		Emerald,
		Sapphire,
		Opal,
	}

	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public enum ItemType
	{
		Passive,
		Active,
	}

	[Header("Item Info")]

	[SerializeField] new string name;
	[TextArea(3, 5)] [Tooltip("{0} = Buff name, \n{1} = Damage, \n{2} = Duration, \n{3} = Cooldown")]
	[SerializeField] string description;

	[Space(10)]
	[Header("Item Properties")]

	[SerializeField] Rarity rarity;
	[Tooltip("The type of item. \nSome items are passive, while others are active. Active Items have additional fields.")]
	[SerializeField] ItemType type;

	[Space(5)]
	GameObject attributesParent;
	[Space(5)]
	[Tooltip("Whether or not this item has attributes such as damage, duration, and cooldown. \nUse the scriptable object to set these values.")]
	[ShowIf(nameof(type), ItemType.Active)]
	[SerializeField] ItemAttributes active;
	[EndIf]
	[Space(10)]
	[ShowIf(nameof(type), ItemType.Passive)]
	[SerializeField] ItemAttributes passive;

	TMP_Text descriptionText;

	public void Invoke()
	{
		// ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
		switch (type)
		{
			case ItemType.Active:
				active.Invoke();
				break;

			case ItemType.Passive:
				passive.Invoke();
				break;
		}
	}

	Color color;

	public void OnValidate()
	{
		if (transform.childCount == 0) return;

		if (!string.IsNullOrEmpty(name)) gameObject.name = name;
		else name = gameObject.name;

		attributesParent = transform.GetComponentInChildren<VerticalLayoutGroup>(true).gameObject;

		// update title text
		attributesParent.transform.parent.GetChild(0).GetComponent<TMP_Text>().text = name;
		RefreshDescription();

		#region Properties
		// set the color based on the name of the rarity
		color = rarity switch
		{ Rarity.Ruby     => new (1f, 0.3f, 0.35f),
		  Rarity.Garnet   => new (1f, 0.7f, 0.5f),
		  Rarity.Emerald  => new (0.15f, 1f, 0.45f),
		  Rarity.Sapphire => new (0.1f, 0.6f, 1f),
		  Rarity.Opal     => new (0.72f, 0.68f, 0.97f),
		  _               => Color.white };

		var background = transform.GetChild(0).GetComponent<Image>();
		Color bgColor = color * .1f;
		bgColor.a = 1f;
		background.color = bgColor;

		var outline = transform.GetChild(0).GetComponent<Outline>();
		outline.effectColor = color;
		#endregion

		#region Attributes
		attributesParent.SetActive(type == ItemType.Active);

		if (type == ItemType.Active)
		{
			var damageText = attributesParent.transform.GetChild(0).GetComponentInChildren<TMP_Text>();
			var durationText = attributesParent.transform.GetChild(1).GetComponentInChildren<TMP_Text>();
			var cooldownText = attributesParent.transform.GetChild(2).GetComponentInChildren<TMP_Text>();

			if (active == null) return;
			active.damageText = damageText;
			active.durationText = durationText;
			active.cooldownText = cooldownText;

			active.Item = this;
		}
		#endregion
	}

	public void RefreshDescription()
	{
		descriptionText = attributesParent.transform.parent.GetChild(2).GetComponent<TMP_Text>();

		descriptionText.text = type switch
		{ ItemType.Active  => string.Format(description, active?.Effect.StatusName, active?.Damage, active?.Duration, active?.Cooldown),
		  ItemType.Passive => string.Format(description, passive?.Effect.StatusName, passive?.Damage, passive?.Duration, passive?.Cooldown),
		  _                => descriptionText.text };
	}
}
