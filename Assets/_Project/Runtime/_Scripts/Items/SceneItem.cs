#region
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#endregion

/// <summary>
///     This class is used to display the item in the scene with the appropriate UI and variables.
/// </summary>
[ExecuteInEditMode]
public class SceneItem : MonoBehaviour
{
	[Tooltip("The item this scene item represents")]
	[SerializeField] Item representedItem;
	[Space(5)] 
	[Header("UI")]
	[SerializeField] TextMeshProUGUI descriptionText;

	/// <summary>
	/// The item this scene item represents.
	/// </summary>
	public Item RepresentedItem => representedItem;

	new public string name => $"{representedItem.name} ({representedItem.RarityColor.rarity.ToString()})";
	public override string ToString() => name;

	void Start()
	{
		Debug.Assert(representedItem != null, $"[SceneItem] {name} does not have a represented item.");
	}

	void Update()
	{
		#region Rarity
		Transform background = transform.GetChild(0);
		if (background == null) return;
		background.GetComponent<Outline>().effectColor = representedItem.RarityColor.color;
		#endregion

		#region Description
		string[] keywords =
		{ "[damage]", "[duration]", "[cooldown]", "[buff]" };

		string format = representedItem.Description;

		for (int i = 0; i < keywords.Length; i++)
			if (representedItem.Description.Contains(keywords[i]))
				format = format.Replace(keywords[i], $"{{{i}}}");

		descriptionText.text = representedItem.Buff != null ? string.Format(format, representedItem.Damage, representedItem.Duration, representedItem.Cooldown, representedItem.Buff.StatusName) : string.Format(format, representedItem.Damage, representedItem.Duration, representedItem.Cooldown);
		#endregion
	}
}
