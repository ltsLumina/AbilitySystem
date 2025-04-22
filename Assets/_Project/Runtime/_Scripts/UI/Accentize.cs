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
	[SerializeField] bool isMarker;
	
	void Start()
	{
		if (TryGetComponentInParent(out Player player))
		{
			Recolour(player.AccentColour);
			return;
		}
		
		associatedBoss = GameManager.Instance.CurrentBoss;
		if (!associatedBoss) return; // Note: this is only for duo bosses, which aren't supported anyways.
		Recolour(associatedBoss.AccentColour);
	}

	public void Recolour(Color accentColor)
	{
		if (isMarker)
		{
			float alpha = target.GetComponent<SpriteRenderer>().color.a;
			accentColor = new (accentColor.r, accentColor.g, accentColor.b, alpha);
		}
		
		if (target.TryGetComponent(out Outline outline)) outline.effectColor = accentColor;
		else if (target.TryGetComponent(out Image image)) image.color = accentColor;
		else if (target.TryGetComponent(out SpriteRenderer spriteRenderer)) spriteRenderer.color = accentColor;
		else if (target.TryGetComponent(out TextMeshProUGUI textMeshPro))
		{
			if (textMeshPro.fontMaterial.HasProperty(ShaderUtilities.ID_OutlineColor)) textMeshPro.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, accentColor);
		}
		else Debug.LogWarning($"No Accentize component found on {gameObject.name}");
	}

	#region Utility
	bool TryGetComponentInParent<T>(out T component)
	{
		component = GetComponentInParent<T>();
		if (component != null) return true;

		component = GetComponent<T>();
		return component != null;
	}
	#endregion
}