#region
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#endregion

/// <summary>
///     This class is used to display the item in the scene with the appropriate UI and variables.
/// </summary>
[ExecuteInEditMode]
public class SceneItem : MonoBehaviour, ISelectHandler, IDeselectHandler
{
	[Tooltip("The item this scene item represents")]
	[SerializeField] Item representedItem;
	[Space(5)]
	[Header("UI")]
	[SerializeField] Image background;
	[SerializeField] TMP_Text nameText;
	[SerializeField] TextMeshProUGUI descriptionText;
	[SerializeField] Sprite sprite;

	/// <summary>
	/// The item this scene item represents.
	/// </summary>
	public Item RepresentedItem => representedItem;
	public Image Sprite => background.transform.GetChild(1).GetComponent<Image>();

	new public string name => $"{representedItem.name} ({representedItem.RarityColor.rarity.ToString()})";
	public override string ToString() => name;

	public void OnSelect(BaseEventData eventData)
	{
		var player = eventData.currentInputModule.GetComponentInParent<Player>();

		// spawn selection marker
		var prefab = Resources.Load<Selection>("PREFABS/UI/Selection");
		var selectionMarker = Instantiate(prefab, transform);
		selectionMarker.Set(player);
	}

	public void OnDeselect(BaseEventData eventData)
	{
		var player = eventData.currentInputModule.GetComponentInParent<Player>();
		
		// save the selection that has the same player as the one that was deselected
		var selectionMarker = GetComponentsInChildren<Selection>().FirstOrDefault(s => s.AssociatedPlayer == player);
		if (selectionMarker == null) return;
		
		Destroy(selectionMarker.gameObject);
	}

	void Start()
	{
		Debug.Assert(representedItem != null, $"[SceneItem] {name} does not have a represented item.");
		Debug.Assert(background != null, $"[SceneItem] {name} does not have a background.");
		Debug.Assert(nameText != null, $"[SceneItem] {name} does not have a name text.");
		Debug.Assert(descriptionText != null, $"[SceneItem] {name} does not have a description text.");
		
		nameText.text = representedItem.Name;
		background.transform.GetChild(1).GetComponent<Image>().sprite = sprite;

		// Get the rarity color and convert it to HSV
		Color.RGBToHSV(representedItem.RarityColor.color, out float h, out float s, out float v);

		// Get the current background color and convert it to HSV
		Color bgColor = background.color;
		Color.RGBToHSV(bgColor, out float _, out float bgS, out float bgV);

		// Create new color with rarity hue but original saturation and value
		background.color = Color.HSVToRGB(h, bgS, bgV);
	}

	void Update()
	{
		#region Rarity
		if (background == null) return;
		background.GetComponent<Outline>().effectColor = representedItem.RarityColor.color;
		#endregion

		#region Description
		string[] keywords =
		{ "[damage]", "[duration]", "[cooldown]", "[buff]" };

		string format = representedItem.Description;

		for (int i = 0; i < keywords.Length; i++)
		{
			if (representedItem.Description.Contains(keywords[i]))
				format = format.Replace(keywords[i], $"{{{i}}}");
		}

		descriptionText.text = representedItem.Buff != null ? string.Format(format, representedItem.Damage, representedItem.Duration, representedItem.Cooldown, representedItem.Buff.StatusName) : string.Format(format, representedItem.Damage, representedItem.Duration, representedItem.Cooldown);
		#endregion
	}
}
