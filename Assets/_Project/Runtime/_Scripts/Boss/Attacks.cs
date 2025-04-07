#region
using JetBrains.Annotations;
using UnityEngine;
#endregion

public class Attacks : MonoBehaviour
{
	#region Burst
	[SerializeField] GameObject orbPrefab;
	[SerializeField] int orbCount = 10;
	[SerializeField] float orbSpeed = 1f;
	[SerializeField] float radius = 1f;
	#endregion

	Entity self;

	void Awake() => self = GetComponent<Entity>();

	[UsedImplicitly]
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

	[UsedImplicitly]
	public void Enrage()
	{
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

			Destroy(orb, 5f);
		}
	}
}
