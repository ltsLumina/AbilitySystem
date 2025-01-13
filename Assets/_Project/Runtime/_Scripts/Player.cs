#region
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using VInspector;
#endregion

public class Player : Entity
{
	[HideInInspector] [UsedImplicitly]
	public VInspectorData data;

	[SerializeField] Job job;
	[Space(10)]
	[SerializeField] int health = 10;

	public Job Job => job;
	public InputManager Inputs => GetComponentInChildren<InputManager>();
	public PlayerInput PlayerInput => Inputs.GetComponent<PlayerInput>();

	protected override void OnTick() { }

	protected override void OnCycle() { }

	void Reset() => gameObject.tag = "Player";

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();

		Inputs.SetDictionaryKeys();
		Job.Abilities.ForEach(a => a.owner = this);

		Logger.Log("Player has spawned.");
	}

	public override void TakeDamage(float damage)
	{
		base.TakeDamage(damage);

		health -= Mathf.RoundToInt(damage);
		if (health <= 0) Logger.Log("Player has died.");
	}

	public void OnHit(Enemy enemy = default)
	{
		health--;

		var sprite = GetComponentInChildren<SpriteRenderer>();
		sprite.FlashSprite(Color.red, 0.3f);
		StartCoroutine(sprite.CreateAfterImages(0.05f, 0.25f, 5));
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		var rb = GetComponentInChildren<Rigidbody2D>();
		Vector2 dir = rb.linearVelocity.normalized * 5;
		Gizmos.DrawRay(transform.position, dir);
		Vector3 point = transform.position + (Vector3) dir;
		Gizmos.DrawWireSphere(point, 0.3f);
	}
}
