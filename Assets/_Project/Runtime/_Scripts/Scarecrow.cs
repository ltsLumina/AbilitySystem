#region
using System;
using System.Collections;
using Lumina.Essentials.Attributes;
using TMPro;
using UnityEngine;
#endregion

public class Scarecrow : MonoBehaviour
{
	enum DPSCheckType
	{
		Continuous,
		TenSeconds,
		HalfMinute,
		Minute,
	}

	[Header("References")] [ReadOnly]
	[SerializeField] Boss associatedBoss;
	[SerializeField] TMP_Text dpsText;

	[Header("DPS Check")]
	[SerializeField] bool checkDPS;
	[SerializeField] DPSCheckType checkType = DPSCheckType.Minute;

	float totalDamage;
	float timeElapsed;

	void Start()
	{
		associatedBoss = GetComponent<Boss>();

		if (!dpsText) dpsText = GameObject.Find("DPS Text").GetComponent<TMP_Text>();
		dpsText.enabled = true;

		if (checkDPS) BeginDPSCheck();
	}

	Coroutine dpsCheckCoroutine;
	float lastDamageTime;

	void Update()
	{
		timeElapsed += Time.deltaTime;
		float dps = totalDamage / Mathf.Max(timeElapsed, 0.001f);
		dpsText.text = "DPS: " + dps.ToString("F2");

		if (checkDPS) dpsCheckCoroutine ??= StartCoroutine(DPSCheckCoroutine());

		// Check if no damage has been registered in the last 5 seconds
		if (Time.time - lastDamageTime > 5f) Cleanse();
	}

	public void RegisterDamage(float amount)
	{
		totalDamage += amount;
		lastDamageTime = Time.time;
		
		BeginDPSCheck();
	}

	void BeginDPSCheck() => dpsCheckCoroutine ??= StartCoroutine(DPSCheckCoroutine());

	IEnumerator DPSCheckCoroutine()
	{
		Logger.Log("Starting DPS check...");

		float dps = 0;

		switch (checkType)
		{
			case DPSCheckType.Continuous:
				while (checkDPS)
				{
					yield return new WaitForSeconds(0.5f);
					dps = totalDamage / Mathf.Max(timeElapsed, 0.001f);
					dpsText.text = "DPS: " + dps.ToString("F2");
				}

				Logger.Log($"DPS over {timeElapsed:F2} seconds: {dps:F2}");
				break;
			
			case DPSCheckType.TenSeconds:
				totalDamage = 0;
				timeElapsed = 0;

				yield return new WaitForSeconds(10);

				dps = totalDamage / Mathf.Max(timeElapsed, 0.001f);
				Logger.Log($"DPS over 10 seconds: {dps:F2}");
				break;

			case DPSCheckType.HalfMinute:
				totalDamage = 0;
				timeElapsed = 0;

				yield return new WaitForSeconds(30);

				dps = totalDamage / Mathf.Max(timeElapsed, 0.001f);
				Logger.Log($"DPS over 30 seconds: {dps:F2}");
				break;

			case DPSCheckType.Minute:
				totalDamage = 0;
				timeElapsed = 0;

				yield return new WaitForSeconds(60);

				dps = totalDamage / Mathf.Max(timeElapsed, 0.001f);
				Logger.Log($"DPS over 60 seconds: {dps:F2}");
				break;

			default:
				throw new ArgumentOutOfRangeException();
		}

		Cleanse();
	}

	void Cleanse()
	{
		associatedBoss.Cleanse();

		totalDamage = 0;
		timeElapsed = 0;
		dpsText.text = "DPS: 0.00";
	}
}
