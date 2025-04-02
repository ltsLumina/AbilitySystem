#region
using System.Collections;
using TMPro;
using UnityEngine;
#endregion

public class Dummy : Entity
{
	[SerializeField] TextMeshProUGUI dpsText;

	float damageTaken;
	Coroutine dpsCoroutine;
	float timeSinceLastDamage;
	float totalElapsedTime;

	protected override void OnTick() { }

	protected override void OnCycle() { }

	public override void TakeDamage(float damage)
	{
		base.TakeDamage(damage);

		damageTaken += damage;
		timeSinceLastDamage = 0f; // Reset the timer when damage is taken

		dpsCoroutine ??= StartCoroutine(CalculateDPS());

		var spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		spriteRenderer.FlashSprite(Color.red, 0.5f);

		PopUpDamageNumbers.ShowDamage(damage, transform.position);
	}

	IEnumerator CalculateDPS()
	{
		timeSinceLastDamage = 0f;
		totalElapsedTime = 0f;

		while (true)
		{
			yield return new WaitForSeconds(0.5f);
			timeSinceLastDamage += 0.5f;
			totalElapsedTime += 0.5f;

			if (timeSinceLastDamage >= 3f)
			{
				damageTaken = 0;
				timeSinceLastDamage = 0f;
				totalElapsedTime = 0f;
			}

			float dps;

			if (totalElapsedTime > 0) dps = damageTaken / totalElapsedTime;
			else
			{
				dps = 0;
				Cleanse();
			}

			dpsText.text = $"DPS: {dps:0.0}";
		}
	}

	void Cleanse()
	{
		damageTaken = 0;
		timeSinceLastDamage = 0;
		totalElapsedTime = 0;

		// TODO: Remove Debuffs
	}

	void OnGUI()
	{
		if (statusEffects.Count == 0) return;

		// on the right of the middle of the screen
		GUILayout.BeginArea(new (Screen.width - 200, Screen.height / 2f - 100, 200, 200));

		foreach (StatusEffect effect in statusEffects)
		{
			if (effect == null) continue;

			GUILayout.Label($"{effect.StatusName} - {effect.Time.RoundTo(1)}");
		}

		GUILayout.EndArea();
	}
}
