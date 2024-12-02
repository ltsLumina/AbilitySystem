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
		[InspectorName("Grey")] Common,    // Grey
		[InspectorName("Blue")] Rare,      // Blue
		[InspectorName("Purple")] Epic,    // Purple
		[InspectorName("Gold")] Legendary, // Gold
		[InspectorName("Red")] Mythic,     // Red
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
	[SerializeField] Rarity rarity;
	[Header("Item Type")]

	[Tooltip("The type of item. \nSome items are passive, while others are active. Active Items have additional fields.")]
	[SerializeField] ItemType type;

	[Space(10)]
	[Tooltip("Whether or not this item has attributes such as damage, duration, and cooldown. \nUse the scriptable object to set these values.")]
	[HideIf(nameof(type), ItemType.Passive)]
	[SerializeField] bool hasAttributes;
	[SerializeField] ItemAttributes attributes;

	[Header("Item Attributes")]

	[SerializeField] GameObject attributesParent;

	TMP_Text descriptionText;

	public void Invoke()
	{
		if (type == ItemType.Active) attributes.Invoke();
		else Logger.LogWarning($"{name} is a passive item and cannot be invoked.");
	}

	public void OnValidate()
	{
		if (!string.IsNullOrEmpty(name)) gameObject.name = name;
		else name = gameObject.name;

		var check = transform.GetComponentInChildren<VerticalLayoutGroup>(true);
		if (!check) return;

		attributesParent = check.gameObject;
		attributesParent.SetActive(hasAttributes);

		if (hasAttributes)
		{
			descriptionText = attributesParent.transform.parent.GetChild(2).GetComponent<TMP_Text>();
			var damageText = attributesParent.transform.GetChild(0).GetComponentInChildren<TMP_Text>();
			var durationText = attributesParent.transform.GetChild(1).GetComponentInChildren<TMP_Text>();
			var cooldownText = attributesParent.transform.GetChild(2).GetComponentInChildren<TMP_Text>();

			RefreshText();

			Debug.Assert(attributes, $"{name} has attributes enabled, but an attribute type has not been set.");
			attributes.damageText = damageText;
			attributes.durationText = durationText;
			attributes.cooldownText = cooldownText;

			attributes.Item = this;
		}
	}

	public void RefreshText() => descriptionText.text = string.Format(description, attributes.Buff.StatusName, attributes.Damage, attributes.Duration, attributes.Cooldown);
}
