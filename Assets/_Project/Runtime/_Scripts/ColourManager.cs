#region
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#endregion

public class ColourManager : Singleton<ColourManager>
{
	public void Initialize(Boss boss)
	{
		GameObject[] accentables = GameObject.FindGameObjectsWithTag("Accentable");

		foreach (GameObject accentable in accentables)
		{
			var spriteRenderer = accentable.GetComponent<SpriteRenderer>();
			if (spriteRenderer != null) spriteRenderer.color = boss.AccentColour;

			var image = accentable.GetComponent<Image>();
			if (image != null) image.color = boss.AccentColour;

			var outline = accentable.GetComponent<Outline>();
			if (outline != null) outline.effectColor = boss.AccentColour;

			var textMeshPro = accentable.GetComponent<TextMeshProUGUI>();
			if (textMeshPro != null) textMeshPro.color = boss.AccentColour;
		}
	}

	public void Initialize(Boss boss, GameObject[] accentables)
	{
		foreach (GameObject accentable in accentables)
		{
			var spriteRenderer = accentable.GetComponent<SpriteRenderer>();
			if (spriteRenderer != null) spriteRenderer.color = boss.AccentColour;

			var image = accentable.GetComponent<Image>();
			if (image != null) image.color = boss.AccentColour;

			var textMeshPro = accentable.GetComponent<TextMeshProUGUI>();
			if (textMeshPro != null) textMeshPro.color = boss.AccentColour;
		}
	}
}
