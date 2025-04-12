#region
using Lumina.Essentials.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#endregion

public class Accentize : MonoBehaviour
{
	[ReadOnly]
	[SerializeField] Boss associatedBoss;
	[SerializeField] Component target;

	void Start()
	{
		associatedBoss = GameManager.Instance.CurrentBoss;
		if (!associatedBoss) return; // Note: this is only for duo bosses, which aren't supported anyways.
		Recolour(associatedBoss.AccentColour);
	}

	public void Recolour(Color accentColor)
	{
		if (target.TryGetComponent(out Outline outline)) outline.effectColor = accentColor;
		else if (target.TryGetComponent(out Image image)) image.color = accentColor;
		else if (target.TryGetComponent(out TextMeshProUGUI textMeshPro)) textMeshPro.color = accentColor;
		else if (target.TryGetComponent(out SpriteRenderer spriteRenderer)) spriteRenderer.color = accentColor;
		else Debug.LogWarning($"No Accentize component found on {gameObject.name}");
	}
}
