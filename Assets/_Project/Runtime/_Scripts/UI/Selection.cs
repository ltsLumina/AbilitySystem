using Lumina.Essentials.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Selection : MonoBehaviour
{
	[ReadOnly]
	[SerializeField] Player associatedPlayer;
	[SerializeField] Image image;
	[SerializeField] TMP_Text text;
	[SerializeField] Color accentColour;

	public Player AssociatedPlayer => associatedPlayer;

	public void Set(Player owner)
	{
		associatedPlayer = owner;
		image = GetComponent<Image>();
		accentColour = owner.AccentColour;
		int index = owner.ID;

		switch (index)
		{
			case 1:
				text.transform.localPosition = new (-52, 60, 0);
				transform.localRotation = Quaternion.Euler(0, 0, 0);
				break;
			
			case 2:
				text.transform.localPosition = new (-52, 60, 0);
				transform.localRotation = Quaternion.Euler(0, 0, 270);
				break;
			
			case 3:
				text.transform.localPosition = new (-62, 55, 0);
				transform.localRotation = Quaternion.Euler(0, 0, 225);
				break;
		}

		image.color = accentColour;
		text.text = owner.Job.Tricode; // TODO: Eventually this will be player name abbreviated.
	}
}
