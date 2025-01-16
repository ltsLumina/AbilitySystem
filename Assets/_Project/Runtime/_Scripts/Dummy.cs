#region
using System.Collections;
using TMPro;
using UnityEngine;
#endregion

public class Dummy : Entity
{
	[SerializeField] NetworkSlider networkSlider;
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

		currentHealth -= damage;
		networkSlider.UpdateSlider(currentHealth / maxHealth);

		if (currentHealth <= 0) Debug.LogWarning($"{gameObject.name} has died!");

		dpsCoroutine ??= StartCoroutine(CalculateDPS());

		var spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		spriteRenderer.FlashSprite(Color.red, 0.5f);

		var damagePopupManager = FindFirstObjectByType<DamagePopupManager>();
		if (damagePopupManager.IsServer) damagePopupManager.SpawnPopUpText(damage, transform.position);
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
}
