#region
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#endregion

[ExecuteInEditMode]
public class SceneItem : MonoBehaviour
{
	[SerializeField] Item item;

	[Space(5)] [Header("UI")]

	[SerializeField] TextMeshProUGUI descriptionText;

	void Update()
	{
		#region Rarity
		Transform background = transform.GetChild(0);
		background.GetComponent<Outline>().effectColor = item.RarityColor.color;
		#endregion

		#region Description
		string[] keywords =
		{ "[damage]", "[duration]", "[cooldown]", "[buff]" };

		string format = item.Description;

		for (int i = 0; i < keywords.Length; i++)
			if (item.Description.Contains(keywords[i]))
				format = format.Replace(keywords[i], $"{{{i}}}");

		descriptionText.text = item.Buff != null ? string.Format(format, item.Damage, item.Duration, item.Cooldown, item.Buff.StatusName) : string.Format(format, item.Damage, item.Duration, item.Cooldown);
		#endregion
	}
}
