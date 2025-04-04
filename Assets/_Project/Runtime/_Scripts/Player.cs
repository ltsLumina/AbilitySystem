﻿#region
using System;
using System.Collections.Generic;
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

	[SerializeField] int health = 100;
	[Foldout("Movement")]
	[SerializeField] List<float> speeds = new (2)
	{ 50, 75 };
	[EndFoldout]
	[SerializeField] float topSpeed = 15;
	[SerializeField] float moveDamping = 5;
	[SerializeField] float stopDamping = 15;

	[Header("Other")]

	[SerializeField] bool mouseMove;

	InputManager inputs;
	PlayerInput playerInput;
	Rigidbody2D rb;

	float speed
	{
		get => mouseMove ? speeds[1] : speeds[0];
		set
		{
			if (mouseMove) speeds[1] = value;
			else speeds[0] = value;
		}
	}

	void Awake() // temp
		=> OnDeath += () =>
		{
			if (canRevive)
			{
				// do revive logic
				health = 100;
				Debug.Log("Player has revived.");
			}
			else
			{
				// actually die
				health = 0;
				inputs.ToggleInputLayer("UI");
			}
		};

	public event Action OnDeath;

	public int Health
	{
		get => health;
		private set
		{
			health = value;
			OnDeath?.Invoke();
		}
	}

	bool canRevive => reviveCount > 0;
	int reviveCount;

	public void AddRevives(int count)
	{
		if (count > 1) Debug.LogWarning($"Adding {count} revives to player. Revive count is now {reviveCount + count}.");

		reviveCount += count;
	}

	[Button] [UsedImplicitly]
	void KillPlayer() => Health = 0;

	#region References
	public Job Job => job;
	public InputManager Inputs => GetComponentInChildren<InputManager>();
	public PlayerInput PlayerInput => Inputs.GetComponent<PlayerInput>();
	#endregion

	#region Properties
	public Modifiers Modifiers { get; private set; }
	#endregion

	protected override void OnTick() { }

	protected override void OnCycle() { }

	void Reset() => gameObject.tag = "Player";

	protected override void OnStart()
	{
		inputs = GetComponentInChildren<InputManager>();
		playerInput = inputs.GetComponent<PlayerInput>();
		rb = GetComponent<Rigidbody2D>();
		Modifiers = GetComponent<Modifiers>();

		Rebind(mouseMove);
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

			case false: // Using Keyboard bindings (1, 2, 3, 4)
				playerInput.actions[InputManager.AbilityKeys[0]].ApplyBindingOverride("<Keyboard>/1");
				playerInput.actions[InputManager.AbilityKeys[1]].ApplyBindingOverride("<Keyboard>/2");
				playerInput.actions[InputManager.AbilityKeys[2]].ApplyBindingOverride("<Keyboard>/3");
				playerInput.actions[InputManager.AbilityKeys[3]].ApplyBindingOverride("<Keyboard>/4");
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

	public override void TakeDamage(float damage)
	{
		base.TakeDamage(damage);

		health -= Mathf.RoundToInt(damage);
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
