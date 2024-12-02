#region
using System.Collections;
using TMPro;
using UnityEngine;
#endregion

public abstract class ItemAttributes : ScriptableObject
{
	[Header("Active Item Properties")]

	[SerializeField] StatusEffect effect;
	[SerializeField] int damage;
	[SerializeField] float duration; // The duration of the potential effect the item has.
	[SerializeField] float cooldown;

	public TMP_Text damageText { get; set; }
	TMP_Text damageValueText { get; set; }
	public TMP_Text durationText { get; set; }
	TMP_Text durationValueText { get; set; }
	public TMP_Text cooldownText { get; set; }
	TMP_Text cooldownValueText { get; set; }

	public StatusEffect Effect
	{
		get => effect;
		set => effect = value;
	}
	public int Damage
	{
		get => damage;
		set => damage = value;
	}

	public float Duration
	{
		get => duration;
		set => duration = value;
	}

	public float Cooldown
	{
		get => cooldown;
		set => cooldown = value;
	}
	public Item Item { get; set; }

	void OnValidate()
	{
		if (damageText == null || durationText == null || cooldownText == null) return;

		damageValueText = damageText.transform.GetChild(0).GetComponent<TMP_Text>();
		durationValueText = durationText.transform.GetChild(0).GetComponent<TMP_Text>();
		cooldownValueText = cooldownText.transform.GetChild(0).GetComponent<TMP_Text>();

		damageValueText.text = Damage.ToString();
		durationValueText.text = $"({Duration}s)";
		cooldownValueText.text = $"{Cooldown}s";

		Item.RefreshDescription();
	}

	public void Invoke()
	{
		Item.StartCoroutine(Wait());

		return;

		IEnumerator Wait()
		{
			yield return new WaitForSeconds(Cooldown);
			OnInvoke();
		}
	}

	protected virtual void OnInvoke() { }
}
