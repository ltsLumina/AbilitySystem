﻿#region
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;

// ReSharper disable UnusedMember.Global
#endregion

public class Attacks : MonoBehaviour
{
	[SerializeField] GameObject orbPrefab;
	[SerializeField] int orbCount = 10;
	[SerializeField] float orbSpeed = 1f;
	[SerializeField] float radius = 1f;

	Entity self;

	void Awake() => self = GetComponent<Entity>();

	public void Burst()
	{
		for (int i = 0; i < orbCount; i++)
		{
			float angle = i * Mathf.PI * 2 / orbCount;
			Vector2 position = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
			GameObject orb = Instantiate(orbPrefab, self.transform.position + (Vector3) position, Quaternion.identity);
			var rb = orb.GetComponent<Rigidbody2D>();
			rb.linearVelocity = position.normalized * orbSpeed;
		}
	}

	public void Spiral(Vector2 origin, bool showWarning, float delay)
	{
		int totalBursts = 8;      // Number of bursts (quarter rotations)
		int orbCount = 10;        // Number of orbs per burst
		float orbSpeed = 5f;      // Speed of the orbs
		float radius = 1f;        // Radius of the burst
		float rotationStep = 90f; // Degrees to rotate per burst

		StartCoroutine(SpiralRoutine());

		return;

		IEnumerator SpiralRoutine()
		{
			for (int burst = 0; burst < totalBursts; burst++)
			{
				float rotationOffset = burst * rotationStep * Mathf.Deg2Rad;

				for (int i = 0; i < orbCount; i++)
				{
					float angle = i * Mathf.PI * 2 / orbCount + rotationOffset;
					Vector2 position = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
					GameObject orb = Instantiate(orbPrefab, origin + position, Quaternion.identity);
					var rb = orb.GetComponent<Rigidbody2D>();
					rb.linearVelocity = position.normalized * orbSpeed;
				}

				yield return new WaitForSeconds(delay);
			}
		}
	}

	public void Spiral4(Vector2 origin, bool showWarning, float delay)
	{
		int totalBursts = 4;      // Number of bursts (quarter rotations)
		int orbCount = 4;         // Number of orbs per burst
		float orbSpeed = 2.5f;    // Speed of the orbs
		float radius = 0.25f;     // Radius of the burst
		float rotationStep = 45f; // Degrees to rotate per burst

		StartCoroutine(SpiralRoutine());

		return;
		IEnumerator SpiralRoutine()
		{
			for (int burst = 0; burst < totalBursts; burst++)
			{
				float rotationOffset = burst * rotationStep * Mathf.Deg2Rad;

				for (int i = 0; i < orbCount; i++)
				{
					float angle = i * Mathf.PI * 2 / orbCount + rotationOffset;
					Vector2 position = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
					GameObject orb = Instantiate(orbPrefab, origin + position, Quaternion.identity);
					var rb = orb.GetComponent<Rigidbody2D>();
					rb.linearVelocity = position.normalized * orbSpeed;
				}

				yield return new WaitForSeconds(delay);
			}
		}
	}

	public void Spiral12(Vector2 origin, bool showWarning, float delay)
	{
		int totalBursts = 12;     // Number of bursts (quarter rotations)
		int orbCount = 6;         // Number of orbs per burst
		float orbSpeed = 2.5f;    // Speed of the orbs
		float radius = 0.25f;     // Radius of the burst
		float rotationStep = 45f; // Degrees to rotate per burst

		StartCoroutine(SpiralRoutine());

		return;
		IEnumerator SpiralRoutine()
		{
			for (int burst = 0; burst < totalBursts; burst++)
			{
				float rotationOffset = burst * rotationStep * Mathf.Deg2Rad;

				for (int i = 0; i < orbCount; i++)
				{
					float angle = i * Mathf.PI * 2 / orbCount + rotationOffset;
					Vector2 position = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
					GameObject orb = Instantiate(orbPrefab, origin + position, Quaternion.identity);
					var rb = orb.GetComponent<Rigidbody2D>();
					rb.linearVelocity = position.normalized * orbSpeed;
				}

				yield return new WaitForSeconds(delay);
			}
		}
	}

	public void Spiral24(Vector2 origin, bool showWarning, float delay)
	{
		int totalBursts = 24;     // Number of bursts (quarter rotations)
		int orbCount = 12;        // Number of orbs per burst
		float orbSpeed = 2.5f;    // Speed of the orbs
		float radius = 0.25f;     // Radius of the burst
		float rotationStep = 45f; // Degrees to rotate per burst

		StartCoroutine(SpiralRoutine());

		return;
		IEnumerator SpiralRoutine()
		{
			for (int burst = 0; burst < totalBursts; burst++)
			{
				float rotationOffset = burst * rotationStep * Mathf.Deg2Rad;

				for (int i = 0; i < orbCount; i++)
				{
					float angle = i * Mathf.PI * 2 / orbCount + rotationOffset;
					Vector2 position = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
					GameObject orb = Instantiate(orbPrefab, origin + position, Quaternion.identity);
					var rb = orb.GetComponent<Rigidbody2D>();
					rb.linearVelocity = position.normalized * orbSpeed;
				}

				yield return new WaitForSeconds(delay);
			}
		}
	}

	public void Donut(Vector2 origin, bool showWarning, float delay)
	{
		int orbCount = 100;
		float orbSpeed = 10f;
		float radius = 5f;

		StartCoroutine(Yield());
		Countdown("get in the circle!", origin, showWarning, delay);

		var markerPrefab = Resources.Load<GameObject>("PREFABS/Boss VFX/Donut AoE");
		GameObject marker = Instantiate(markerPrefab, origin, Quaternion.identity);
		Destroy(marker, delay);

		return;

		IEnumerator Yield()
		{
			yield return new WaitForSeconds(delay);

			for (int i = 0; i < orbCount; i++)
			{
				float angle = i * Mathf.PI * 2 / orbCount;
				Vector2 position = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
				GameObject orb = Instantiate(orbPrefab, origin + position, Quaternion.identity);
				var rb = orb.GetComponent<Rigidbody2D>();
				rb.linearVelocity = position.normalized * orbSpeed;
			}
		}
	}
	
	readonly Queue<Sequence> countdowns = new ();

	/// <summary>
	///     Spawns a 'dynamic' countdown that follows the player for a given delay while displaying the countdown message.
	/// </summary>
	void Countdown(string message, bool showWarning, float delay)
	{
		if (!showWarning) return;
		
		ReadOnlyArray<Player> players = PlayerManager.Instance.Players;
		var countdownPrefab = Resources.Load<GameObject>("PREFABS/Boss VFX/AoE Countdown");
		Transform parent = GameObject.FindWithTag("Worldspace Canvas").transform;
		var offset = new Vector3(0, Mathf.Max(countdowns.Count + 1f, 2f));

		foreach (Player player in players)
		{
			GameObject countdown = Instantiate(countdownPrefab, player.transform.position + offset, Quaternion.identity, parent);
			countdown.name = $"Countdown: \"{message}\" (dynamic) | (#{countdowns.Count})";
			Tween moveTween = countdown.transform.DOMove(player.transform.position + offset, delay).SetEase(Ease.Linear).OnUpdate(() => countdown.transform.position = player.transform.position + offset).SetLink(gameObject);

			var tmp = countdown.GetComponent<TextMeshProUGUI>();
			tmp.text = message;

			// set the progress of the image over delay time
			Transform backgroundImage = countdown.transform.GetChild(0);
			Transform progressImage = backgroundImage.GetChild(0);
			var background = backgroundImage.GetComponent<Image>();
			var progress = progressImage.GetComponent<Image>();

			Sequence sequence = DOTween.Sequence();
			countdowns.Enqueue(sequence);
			sequence.SetLink(gameObject);
			sequence.OnStart(() => progress.fillAmount = 0f);
			sequence.Append(progress.DOFillAmount(1f, delay).SetEase(Ease.Linear));
			sequence.AppendInterval(0.8f);
			sequence.Join(tmp.DOFade(0, 0.2f).SetEase(Ease.Linear));
			sequence.Join(progress.DOFade(0, 0.2f).SetEase(Ease.Linear));
			sequence.Join(background.DOFade(0, 0.2f).SetEase(Ease.Linear));

			sequence.OnComplete
			(() =>
			{
				countdowns.Dequeue();
				moveTween?.Kill();
				Destroy(countdown, 0.1f);
			});
		}
	}

	/// <summary>
	///     Spawns a 'static' countdown prefab at a given position and displays the countdown message.
	///     <para>Used for AoEs that have a static position, or for AoEs that don't follow the player.</para>
	///     <example> Donut, Line, Cleave, etc.</example>
	/// </summary>
	void Countdown(string message, Vector2 origin, bool showWarning, float delay) // note for self: this will be for countdowns that use a static position
	{
		if (!showWarning) return;
		
		var countdownPrefab = Resources.Load<GameObject>("PREFABS/Boss VFX/AoE Countdown");
		Transform parent = GameObject.FindWithTag("Worldspace Canvas").transform;
		Vector2 offset = Vector2.up * 2f;

		GameObject countdown = Instantiate(countdownPrefab, origin + offset, Quaternion.identity, parent);
		countdown.name = $"Countdown: \"{message}\" @ {origin} | (#{countdowns.Count})";

		var tmp = countdown.GetComponent<TextMeshProUGUI>();
		tmp.text = message;

		// set the progress of the image over delay time
		Transform backgroundImage = countdown.transform.GetChild(0);
		Transform progressImage = backgroundImage.GetChild(0);
		var background = backgroundImage.GetComponent<Image>();
		var progress = progressImage.GetComponent<Image>();

		Sequence sequence = DOTween.Sequence();
		sequence.SetLink(gameObject);
		sequence.OnStart(() => progress.fillAmount = 0f);
		sequence.Append(progress.DOFillAmount(1f, delay).SetEase(Ease.Linear));
		sequence.AppendInterval(0.8f);
		sequence.Join(tmp.DOFade(0, 0.2f).SetEase(Ease.Linear));
		sequence.Join(progress.DOFade(0, 0.2f).SetEase(Ease.Linear));
		sequence.Join(background.DOFade(0, 0.2f).SetEase(Ease.Linear));
		sequence.OnComplete(() => Destroy(countdown));
	}

	public void Line(Vector2 origin, bool showWarning, float delay)
	{
		var linePrefab = Resources.Load<GameObject>("PREFABS/Boss VFX/Line AoE");
		GameObject line = Instantiate(linePrefab, origin, Quaternion.identity);
		line.transform.localScale = new (Screen.width / 100f, line.transform.localScale.y, line.transform.localScale.z);
		Destroy(line, delay + 0.1f);

		StartCoroutine(Anim());
		StartCoroutine(Yield());

		Countdown("get away from the line!", origin, showWarning, delay);

		return;
		IEnumerator Anim()
		{
			var spriteRenderer = line.GetComponentInChildren<SpriteRenderer>();
			yield return new WaitForSeconds(delay - 0.2f);

			Sequence sequence = DOTween.Sequence();
			sequence.SetLink(gameObject);
			sequence.Append(spriteRenderer.DOFade(1, 0.2f).SetEase(Ease.Linear));
			sequence.Join(spriteRenderer.transform.DOScaleY(0, 0.2f).SetEase(Ease.InOutCubic));
			sequence.AppendInterval(0.1f);
			sequence.OnComplete(() => Destroy(line, 0.1f));
		}

		IEnumerator Yield()
		{
			yield return new WaitForSeconds(delay);

			if (line == null) yield break;

			var collider = line.GetComponent<BoxCollider2D>();

			var results = new List<Collider2D>();
			var contactFilter = new ContactFilter2D();
			contactFilter.SetLayerMask(LayerMask.GetMask("Player"));
			collider.Overlap(contactFilter, results);

			foreach (Collider2D col in results)
			{
				if (col.TryGetComponent(out Player player))
				{
					player.TakeDamage(1);
					line.SetActive(false);
				}
				else line.SetActive(false);
			}
		}
	}

	public void LineBig(Vector2 origin, bool showWarning, float delay)
	{
		var linePrefab = Resources.Load<GameObject>("PREFABS/Boss VFX/Line AoE");
		GameObject line = Instantiate(linePrefab, origin, Quaternion.identity);
		line.transform.localScale = new (Screen.width / 100f, line.transform.localScale.y * 2, line.transform.localScale.z);

		StartCoroutine(Anim());
		StartCoroutine(Yield());

		Countdown("get away from the line!", origin, showWarning, delay);

		return;
		IEnumerator Anim()
		{
			var spriteRenderer = line.GetComponentInChildren<SpriteRenderer>();
			yield return new WaitForSeconds(delay - 0.2f);

			Sequence sequence = DOTween.Sequence();
			sequence.SetLink(gameObject);
			sequence.Append(spriteRenderer.DOFade(1, 0.2f).SetEase(Ease.Linear));
			sequence.Join(spriteRenderer.transform.DOScaleY(0, 0.2f).SetEase(Ease.InOutCubic));
			sequence.AppendInterval(0.1f);
			sequence.OnComplete(() => Destroy(line, 0.1f));
		}

		IEnumerator Yield()
		{
			yield return new WaitForSeconds(delay);

			if (line == null) yield break;

			var collider = line.GetComponent<BoxCollider2D>();

			var results = new List<Collider2D>();
			var contactFilter = new ContactFilter2D();
			contactFilter.SetLayerMask(LayerMask.GetMask("Player"));
			collider.Overlap(contactFilter, results);

			foreach (Collider2D col in results)
			{
				if (col.TryGetComponent(out Player player))
				{
					player.TakeDamage(1);
					line.SetActive(false);
				}
				else line.SetActive(false);
			}
		}
	}

	public void Pillar(Vector2 origin, bool showWarning, float delay)
	{
		var linePrefab = Resources.Load<GameObject>("PREFABS/Boss VFX/Line AoE");
		GameObject line = Instantiate(linePrefab, origin, Quaternion.identity);
		line.transform.localScale = new (Screen.width / 100f, line.transform.localScale.y, line.transform.localScale.z);
		line.transform.rotation = Quaternion.Euler(0, 0, 90);

		StartCoroutine(Anim());
		StartCoroutine(Yield());

		Countdown("get away from the line!", origin, showWarning, delay);

		return;
		IEnumerator Anim()
		{
			var spriteRenderer = line.GetComponentInChildren<SpriteRenderer>();
			yield return new WaitForSeconds(delay - 0.2f);

			Sequence sequence = DOTween.Sequence();
			sequence.SetLink(gameObject);
			sequence.Append(spriteRenderer.DOFade(1, 0.2f).SetEase(Ease.Linear));
			sequence.Join(spriteRenderer.transform.DOScaleY(0, 0.2f).SetEase(Ease.InOutCubic));
			sequence.AppendInterval(0.1f);
			sequence.OnComplete(() => Destroy(line, 0.1f));
		}

		IEnumerator Yield()
		{
			yield return new WaitForSeconds(delay);

			if (line == null) yield break;

			var collider = line.GetComponent<BoxCollider2D>();

			var results = new List<Collider2D>();
			var contactFilter = new ContactFilter2D();
			contactFilter.SetLayerMask(LayerMask.GetMask("Player"));
			collider.Overlap(contactFilter, results);

			foreach (Collider2D col in results)
			{
				if (col.TryGetComponent(out Player player))
				{
					player.TakeDamage(1);
					line.SetActive(false);
				}
				else line.SetActive(false);
			}
		}
	}

	public void Line45(Vector2 origin, bool showWarning, float delay)
	{
		var linePrefab = Resources.Load<GameObject>("PREFABS/Boss VFX/Line AoE");
		GameObject line = Instantiate(linePrefab, origin, Quaternion.identity);
		line.transform.localScale = new (Screen.width / 100f, line.transform.localScale.y, line.transform.localScale.z);
		line.transform.rotation = Quaternion.Euler(0, 0, 45);

		StartCoroutine(Anim());
		StartCoroutine(Yield());

		Countdown("get away from the line!", origin, showWarning, delay);

		return;

		IEnumerator Anim()
		{
			var spriteRenderer = line.GetComponentInChildren<SpriteRenderer>();
			yield return new WaitForSeconds(delay - 0.2f);

			Sequence sequence = DOTween.Sequence();
			sequence.SetLink(gameObject);
			sequence.Append(spriteRenderer.DOFade(1, 0.2f).SetEase(Ease.Linear));
			sequence.Join(spriteRenderer.transform.DOScaleY(0, 0.2f).SetEase(Ease.InOutCubic));
			sequence.OnComplete(() => Destroy(line, 0.1f));
		}

		IEnumerator Yield()
		{
			yield return new WaitForSeconds(delay);

			if (line == null) yield break;

			var collider = line.GetComponent<BoxCollider2D>();

			var results = new List<Collider2D>();
			collider.Overlap(results);

			foreach (Collider2D col in results)
			{
				if (col.TryGetComponent(out Player player))
				{
					player.TakeDamage(1);
					line.SetActive(false);
				}
				else line.SetActive(false);
			}
		}
	}

	public void Line315(Vector2 origin, bool showWarning, float delay)
	{
		var linePrefab = Resources.Load<GameObject>("PREFABS/Boss VFX/Line AoE");
		GameObject line = Instantiate(linePrefab, origin, Quaternion.identity);
		line.transform.localScale = new (Screen.width / 100f, line.transform.localScale.y, line.transform.localScale.z);
		line.transform.rotation = Quaternion.Euler(0, 0, 315);

		StartCoroutine(Anim());
		StartCoroutine(Yield());

		Countdown("get away from the line!", origin, showWarning, delay);

		return;

		IEnumerator Anim()
		{
			var spriteRenderer = line.GetComponentInChildren<SpriteRenderer>();
			yield return new WaitForSeconds(delay - 0.2f);

			Sequence sequence = DOTween.Sequence();
			sequence.SetLink(gameObject);
			sequence.Append(spriteRenderer.DOFade(1, 0.2f).SetEase(Ease.Linear));
			sequence.Join(spriteRenderer.transform.DOScaleY(0, 0.2f).SetEase(Ease.InOutCubic));
			sequence.OnComplete(() => Destroy(line, 0.1f));
		}

		IEnumerator Yield()
		{
			yield return new WaitForSeconds(delay);

			if (line == null) yield break;

			var collider = line.GetComponent<BoxCollider2D>();

			var results = new List<Collider2D>();
			collider.Overlap(results);

			foreach (Collider2D col in results)
			{
				if (col.TryGetComponent(out Player player))
				{
					player.TakeDamage(1);
					line.SetActive(false);
				}
				else line.SetActive(false);
			}
		}
	}

	public void CleaveUp(Vector2 origin, bool showWarning, float delay) => PerformCleave(origin, showWarning, delay, 0f);

	public void CleaveDown(Vector2 origin, bool showWarning, float delay) => PerformCleave(origin, showWarning, delay, 180f);

	public void CleaveLeft(Vector2 origin, bool showWarning, float delay) => PerformCleave(origin, showWarning, delay, 90f);

	public void CleaveRight(Vector2 origin, bool showWarning, float delay) => PerformCleave(origin, showWarning, delay, 270f);

	public void CleaveTopLeft(Vector2 origin, bool showWarning, float delay) => PerformCleave(origin, showWarning, delay, 45f);

	public void CleaveTopRight(Vector2 origin, bool showWarning, float delay) => PerformCleave(origin, showWarning, delay, 315f);

	public void CleaveBottomLeft(Vector2 origin, bool showWarning, float delay) => PerformCleave(origin, showWarning, delay, 135f);

	public void CleaveBottomRight(Vector2 origin, bool showWarning, float delay) => PerformCleave(origin, showWarning, delay, 225f);

	#region Cleave
	void PerformCleave(Vector2 origin, bool showWarning, float delay, float angle)
	{
		var cleavePrefab = Resources.Load<GameObject>("PREFABS/Boss VFX/Cleave AoE");
		GameObject cleave = Instantiate(cleavePrefab, origin, Quaternion.identity);
		cleave.transform.rotation = Quaternion.Euler(0, 0, angle);

		var graphic = cleave.GetComponentInChildren<SpriteRenderer>();
		graphic.sortingOrder = 1;
		graphic.transform.localScale = new (Screen.width / 100f, graphic.transform.localScale.y, graphic.transform.localScale.z);

		StartCoroutine(CleaveHitRoutine(cleave, delay));
		Countdown("cleaving one side!", showWarning, delay);
	}

	IEnumerator CleaveHitRoutine(GameObject cleave, float delay)
	{
		yield return new WaitForSeconds(delay);
		if (cleave == null) yield break;
		var collider = cleave.GetComponentInChildren<BoxCollider2D>();
		var results = new List<Collider2D>();
		collider.Overlap(results);

		foreach (Collider2D col in results)
		{
			if (col.TryGetComponent(out Player player))
			{
				player.TakeDamage(1);
				Destroy(cleave);
			}
			else Destroy(cleave);
		}
	}

	#region Alternative Cleave
	//public void CleaveLeftAlt(Vector2 origin, float delay) => PerformCleaveAlt(origin, delay, 90f);

	// void PerformCleaveAlt(Vector2 origin, float delay, float angle)
	// {
	//     int orbCount = 20; // Number of orbs in the wall
	//     float spacing = 1f; // Spacing between orbs
	//
	//     var cleavePrefab = Resources.Load<GameObject>("PREFABS/Boss VFX/Cleave AoE");
	//     GameObject cleave = Instantiate(cleavePrefab, origin, Quaternion.identity);
	//     cleave.transform.rotation = Quaternion.Euler(0, 0, angle);
	//
	//     var graphic = cleave.GetComponentInChildren<SpriteRenderer>();
	//     graphic.sortingOrder = 1;
	//     graphic.transform.localScale = new (Screen.width / 100f, graphic.transform.localScale.y, graphic.transform.localScale.z);
	//     
	//     //StartCoroutine(CleaveWallRoutine());
	// }
	#endregion
	#endregion

	public void HSphere(Vector2 origin, bool showWarning, float delay)
	{
		var spherePrefab = Resources.Load<GameObject>("PREFABS/Boss VFX/Sphere AoE");
		GameObject marker = Instantiate(spherePrefab, origin, Quaternion.identity);
		Destroy(marker, delay);

		float dx = -origin.x - origin.x;                    // The x-direction of travel
		float angleH = Mathf.Atan2(0f, dx) * Mathf.Rad2Deg; // 0 or 180
		marker.transform.rotation = Quaternion.Euler(0f, 0f, angleH);
		

		StartCoroutine(Yield());

		return;
		IEnumerator Yield()
		{
			yield return new WaitForSeconds(delay);

			float scale = 10f;
			float duration = 10f;

			GameObject orb = Instantiate(orbPrefab, origin, Quaternion.identity);
			orb.transform.localScale *= scale;
			var rb = orb.GetComponent<Rigidbody2D>();
			rb.linearVelocity = Vector2.zero; // No initial speed

			// Move the orb to the LEFT over time, then fade it out, and finally destroy it
			orb.transform.DOMoveX(-origin.x, duration).SetEase(Ease.Linear).OnComplete(() => orb.GetComponent<SpriteRenderer>().DOFade(0, 1f).OnComplete(() => Destroy(orb)).SetLink(orb.gameObject));
		}
	}

	public void VSphere(Vector2 origin, bool showWarning, float delay)
	{
		var spherePrefab = Resources.Load<GameObject>("PREFABS/Boss VFX/Sphere AoE");
		GameObject marker = Instantiate(spherePrefab, origin, Quaternion.identity);
		Destroy(marker, delay);

		float dy = -origin.y - origin.y;                    // The y-direction of travel
		float angleV = Mathf.Atan2(dy, 0f) * Mathf.Rad2Deg; // 90 or -90
		marker.transform.rotation = Quaternion.Euler(0f, 0f, angleV);
		
		StartCoroutine(Yield());

		return;

		IEnumerator Yield()
		{
			yield return new WaitForSeconds(delay);

			float scale = 10f;
			float duration = 10f;

			GameObject orb = Instantiate(orbPrefab, origin, Quaternion.identity);
			orb.transform.localScale *= scale;
			var rb = orb.GetComponent<Rigidbody2D>();
			rb.linearVelocity = Vector2.zero; // No initial speed

			// Move the orb to the LEFT over time, then fade it out, and finally destroy it
			orb.transform.DOMoveY(-origin.y, duration).SetEase(Ease.Linear).OnComplete(() => orb.GetComponent<SpriteRenderer>().DOFade(0, 1f).OnComplete(() => Destroy(orb)).SetLink(orb.gameObject));
		}
	}

	public void SpawnerUp(Vector2 origin, bool showWarning, float delay) => Spawner(origin, showWarning, Vector2.up, delay);

	public void SpawnerDown(Vector2 origin, bool showWarning, float delay) => Spawner(origin, showWarning, Vector2.down, delay);

	public void SpawnerLeft(Vector2 origin, bool showWarning, float delay) => Spawner(origin, showWarning, Vector2.left, delay);

	public void SpawnerRight(Vector2 origin, bool showWarning, float delay) => Spawner(origin, showWarning, Vector2.right, delay);

	void Spawner(Vector2 origin, bool showWarning, Vector2 direction, float delay)
	{
		StartCoroutine(SpawnRoutine());

		return;

		IEnumerator SpawnRoutine()
		{
			int orbCount = 10;
			float interval = delay / orbCount;

			for (int i = 0; i < orbCount; i++)
			{
				GameObject orb = Instantiate(orbPrefab, origin, Quaternion.identity);
				var rb = orb.GetComponent<Rigidbody2D>();
				rb.linearVelocity = direction.normalized * orbSpeed;

				yield return new WaitForSeconds(interval);
			}
		}
	}

	public void Enrage(float enrageDelay) => StartCoroutine(EnrageCoroutine(enrageDelay));

	IEnumerator EnrageCoroutine(float enrageDelay)
	{
		while (PlayerManager.Instance.Players.Any(p => p.Health > 0))
		{
			Countdown("get ready for the enrage!", new (0, 0), true, enrageDelay);

			int orbCount = 100;
			float orbSpeed = 10f;
			float radius = 0.1f;

			for (int i = 0; i < orbCount; i++)
			{
				float angle = i * Mathf.PI * 2 / orbCount;
				Vector2 position = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
				GameObject orb = Instantiate(orbPrefab, self.transform.position + (Vector3) position, Quaternion.identity);
				var rb = orb.GetComponent<Rigidbody2D>();
				rb.linearVelocity = position.normalized * orbSpeed;
			}

			yield return new WaitForSeconds(enrageDelay);
		}

		Debug.LogWarning("Player(s) has died. Ending enrage.");
	}
}
