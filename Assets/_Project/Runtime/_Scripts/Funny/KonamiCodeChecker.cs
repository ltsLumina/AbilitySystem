#region
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#endregion

public class KonamiCodeChecker : MonoBehaviour
{
	[NonReorderable]
	[SerializeField] List<KeyCode> inputKeys = new ();
	readonly List<KeyCode> konamiCode = new ()
	{ KeyCode.UpArrow,
	  KeyCode.UpArrow,
	  KeyCode.DownArrow,
	  KeyCode.DownArrow,
	  KeyCode.LeftArrow,
	  KeyCode.RightArrow,
	  KeyCode.LeftArrow,
	  KeyCode.RightArrow,
	  KeyCode.B,
	  KeyCode.A };

	bool showGUI;

	void Update() => CheckKonamiCode();

	void OnGUI()
	{
		if (showGUI) StartCoroutine(PopUp());
	}

	void CheckKonamiCode()
	{
		if (Input.anyKeyDown)
		{
			KeyCode[] possibleKeys =
			{ KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.B, KeyCode.A };

			foreach (KeyCode key in possibleKeys)
			{
				if (Input.GetKeyDown(key))
				{
					inputKeys.Add(key);
					break;
				}
			}

			if (inputKeys.Count > konamiCode.Count) inputKeys.RemoveAt(0);

			if (inputKeys.Count == konamiCode.Count && inputKeys.SequenceEqual(konamiCode))
			{
				Debug.Log("Konami Code Entered!");
				inputKeys.Clear();

				showGUI = true;
				StartCoroutine(Wait());

				IEnumerator Wait()
				{
					yield return new WaitForSeconds(1.5f);
					Application.OpenURL("https://www.youtube.com/watch?v=ClHNxPHjhtc");
				}
			}
		}
	}

	IEnumerator PopUp()
	{
		Rect rect = new (Screen.width / 2f - 50, Screen.height / 2f - 25, 500, 500);

		GUI.Label
		(rect, "Konami Code Entered!", new ()
		 { fontSize = 20, fontStyle = FontStyle.Bold, normal =
		   { textColor = Color.white } });

		yield return new WaitForSeconds(2.5f);

		showGUI = false;
	}
}
