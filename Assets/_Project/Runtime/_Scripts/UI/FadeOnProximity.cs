#region
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using static Lumina.Essentials.Modules.Helpers;
#endregion

public class FadeOnProximity : MonoBehaviour
{
	Canvas canvas;

	void Awake() => canvas = GetComponent<Canvas>();

	void Update()
	{
		ReadOnlyArray<Player> players = PlayerManager.Instance.Players;

		float minDistance = float.MaxValue;
		var hotbar = (RectTransform) canvas.transform.GetChild(0);
		Vector2 uiScreenPos = hotbar.position;

		foreach (Player p in players)
		{
			Vector3 playerScreenPos = CameraMain.WorldToScreenPoint(p.transform.position);
			float distance = Vector2.Distance(playerScreenPos, uiScreenPos);
			if (distance < minDistance) minDistance = distance;
		}

		if (GameManager.Instance.CurrentBoss != null)
		{
			Boss boss = GameManager.Instance.CurrentBoss;
			Vector3 bossScreenPos = CameraMain.WorldToScreenPoint(boss.transform.position);
			float bossDistance = Vector2.Distance(bossScreenPos, uiScreenPos);
			if (bossDistance < minDistance) minDistance = bossDistance;
		}

		float fadeStartDistance = 125f;
		float fadeEndDistance = 150f;
		float t = Mathf.InverseLerp(fadeStartDistance, fadeEndDistance, minDistance);
		canvas.GetComponent<CanvasGroup>().alpha = Mathf.Clamp01(t);
	}
}
