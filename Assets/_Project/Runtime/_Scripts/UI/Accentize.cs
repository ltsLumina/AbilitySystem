#region
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#endregion

public class Accentize : MonoBehaviour
{
	[SerializeField] Component target;

	void Awake()
	{
		if (target.TryGetComponent(out Outline outline)) outline.effectColor = GameManager.Instance.CurrentBoss.AccentColour;
		else if (target.TryGetComponent(out Image image)) image.color = GameManager.Instance.CurrentBoss.AccentColour;
		else if (target.TryGetComponent(out TextMeshProUGUI textMeshPro)) textMeshPro.color = GameManager.Instance.CurrentBoss.AccentColour;
		else if (target.TryGetComponent(out SpriteRenderer spriteRenderer)) spriteRenderer.color = GameManager.Instance.CurrentBoss.AccentColour;
		else Debug.LogWarning($"No accentizable component found on {gameObject.name}");
	}
}
