#region
using System.Collections;
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

	[SerializeField] int health = 10;
	[Foldout("Movement")]
	[SerializeField] List<float> speeds = new (2)
	{ 75, 35 };
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

	public Job Job => job;
	public InputManager Inputs => GetComponentInChildren<InputManager>();
	public PlayerInput PlayerInput => Inputs.GetComponent<PlayerInput>();

	protected override void OnTick() { }

	protected override void OnCycle() { }

	void Reset() => gameObject.tag = "Player";

	protected override void OnStart()
	{
		inputs = GetComponentInChildren<InputManager>();
		playerInput = inputs.GetComponent<PlayerInput>();
		rb = GetComponent<Rigidbody2D>();

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
		if (health <= 0) Logger.Log("Player has died.");
	}

	public void OnHit(Enemy enemy = default)
	{
		health--;

		var sprite = GetComponentInChildren<SpriteRenderer>();
		sprite.FlashSprite(Color.red, 0.3f);
		StartCoroutine(sprite.CreateAfterImages(0.05f, 0.25f, 5));

		StartCoroutine(Foo());
	}

	IEnumerator Foo()
	{
		speed *= 1.5f;
		yield return new WaitForSeconds(0.5f);
		speed /= 1.5f;
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
}
