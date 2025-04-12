#region
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using VInspector;
using static Lumina.Essentials.Modules.Helpers;
#endregion

public class Player : Entity
{
	[HideInInspector] [UsedImplicitly]
	public VInspectorData data;

	[SerializeField] Job job;
	[Space(10)]
	[Header("Health")]

	[SerializeField] int health = 3;
	[SerializeField] int maxHealth = 3;
	[SerializeField] float takeDamageCooldown = 1f;
	[Space(5)]
	[Foldout("Movement")]
	[SerializeField] List<float> speeds = new (2)
	{ 50, 75 };
	[EndFoldout]
	[SerializeField] float topSpeed = 15;
	[SerializeField] float moveDamping = 5;
	[SerializeField] float stopDamping = 15;
	[Space(10)]
	[Header("Other")]

	[SerializeField] bool mouseMove;

	InputManager inputs;
	PlayerInput playerInput;
	Rigidbody2D rb;

	float speed
	{
		get => mouseMove ? speeds[1] * Stats.Speed : speeds[0] * Stats.Speed;
		set
		{
			if (mouseMove) speeds[1] = value;
			else speeds[0] = value;
		}
	}

	public event Action OnDeath;

	public int Health
	{
		get => health;
		private set
		{
			health = value;

			if (health <= 0) OnDeath?.Invoke();
			else if (health > maxHealth) health = maxHealth;
		}
	}

	[Button] [UsedImplicitly]
	void KillPlayer() => Health = 0;

	#region References
	public Job Job => job;
	public InputManager Inputs => GetComponentInChildren<InputManager>();
	public PlayerInput PlayerInput => Inputs.GetComponent<PlayerInput>();
	#endregion

	#region Properties
	public Stats Stats { get; private set; }
	#endregion

	protected override void OnTick() { }

	protected override void OnCycle() { }

	void Reset()
	{
		if (!Application.isPlaying) return;
		gameObject.tag = $"Player {playerInput.playerIndex + 1}";
	}

	protected override void OnStart()
	{
		inputs = GetComponentInChildren<InputManager>();
		playerInput = inputs.GetComponent<PlayerInput>();
		rb = GetComponent<Rigidbody2D>();
		Stats = GetComponent<Stats>();

		//Rebind(mouseMove);

		#region Init Player
		Health = maxHealth;

		OnDeath += () =>
		{
			Logger.LogWarning("Player has died!");

			//playerInput.DeactivateInput();
			//playerInput.SwitchCurrentActionMap("UI");

			//StopAllCoroutines();

			//SceneManagerExtended.ReloadScene();
		};
		#endregion
	}

	void Rebind(bool useMouseBindings)
	{
		switch (useMouseBindings)
		{
			case true: // Using Mouse bindings (Q, W, E, R)
				playerInput.actions[InputManager.AbilityKeys[0]].ApplyBindingOverride("<Keyboard>/q");
				playerInput.actions[InputManager.AbilityKeys[1]].ApplyBindingOverride("<Keyboard>/w");
				playerInput.actions[InputManager.AbilityKeys[2]].ApplyBindingOverride("<Keyboard>/e");
				playerInput.actions[InputManager.AbilityKeys[3]].ApplyBindingOverride("<Keyboard>/r");
				break;

			case false: // Using Keyboard bindings (h, j, k, l)
				playerInput.actions[InputManager.AbilityKeys[0]].ApplyBindingOverride("<Keyboard>/h");
				playerInput.actions[InputManager.AbilityKeys[1]].ApplyBindingOverride("<Keyboard>/j");
				playerInput.actions[InputManager.AbilityKeys[2]].ApplyBindingOverride("<Keyboard>/k");
				playerInput.actions[InputManager.AbilityKeys[3]].ApplyBindingOverride("<Keyboard>/l");
				break;
		}
	}

	void FixedUpdate()
	{
		if (mouseMove) MouseMove();
		else Move();
	}

	void Move()
	{
		rb.AddForce(inputs.MoveInput * speed);
		rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, topSpeed);
		bool changingDir = Vector2.Dot(rb.linearVelocity, inputs.MoveInput) < 0;
		rb.linearDamping = changingDir ? stopDamping : moveDamping;
		if (!inputs.IsMoving) rb.linearDamping = Mathf.Lerp(rb.linearDamping, 100, 0.1f);
	}

	void MouseMove()
	{
		inputs.OverrideMoveInput((CameraMain.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized);

		Vector3 mousePos = CameraMain.ScreenToWorldPoint(Input.mousePosition);
		Vector3 dir = (mousePos - transform.position).normalized;
		rb.AddForce(dir * speed);
	}

	protected override void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Projectile")) TakeDamage(1);
	}

	SpriteRenderer sprite => GetComponentInChildren<SpriteRenderer>();

	public override void TakeDamage(float damage)
	{
		if (isOnCooldown) return;

		// Check if the player has shields, and if so, remove one shield and destroy the projectiles surrounding the player
		if (Stats.Shields > 0)
		{
			Stats.Remove("shields", 1);

			DestroyNearbyOrbs();

			sprite.FlashSprite(Color.cyan, 0.3f);
			CameraMain.DOShakePosition(0.15f, 0.5f);
			return;
		}

		// Destroy all projectiles within a radius of 5 units
		DestroyNearbyOrbs();

		Health -= Mathf.RoundToInt(damage);
		StartCoroutine(DamageCooldown());

		base.TakeDamage(damage);

		sprite.FlashSprite(Color.red, 0.3f);
		CameraMain.DOShakePosition(0.3f, 1f);

		return;

		void DestroyNearbyOrbs()
		{
			foreach (Collider2D col in Physics2D.OverlapCircleAll(transform.position, 5))
				if (col.CompareTag("Projectile"))
					Destroy(col.gameObject);
		}
	}

	bool isOnCooldown;

	IEnumerator DamageCooldown()
	{
		isOnCooldown = true;
		yield return new WaitForSeconds(takeDamageCooldown); // Cooldown duration
		isOnCooldown = false;
	}

	public void OnHit(Enemy enemy = null)
	{
		health--;

		var sprite = GetComponentInChildren<SpriteRenderer>();
		sprite.FlashSprite(Color.red, 0.3f);
		StartCoroutine(sprite.CreateAfterImages(0.05f, 0.25f, 5));
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		if (!rb) return;
		Vector2 dir = rb.linearVelocity.normalized * 5;
		Gizmos.DrawRay(transform.position, dir);
		Vector3 point = transform.position + (Vector3) dir;
		Gizmos.DrawWireSphere(point, 0.3f);
	}

	void OnGUI()
	{
		if (statusEffects.Count == 0) return;

		// on the left of the middle of the screen
		GUILayout.BeginArea(new (Screen.width / 2f - 200, Screen.height / 2f - 200, 400, 400));

		foreach (StatusEffect effect in statusEffects)
		{
			if (effect == null) continue;

			GUILayout.Label($"{effect.StatusName} - {effect.Time.RoundTo(1)}");
		}

		GUILayout.EndArea();
	}
}
