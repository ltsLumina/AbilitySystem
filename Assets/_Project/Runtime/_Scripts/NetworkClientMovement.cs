using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using VInspector;
using static Lumina.Essentials.Modules.Helpers;

public class NetworkClientMovement : NetworkBehaviour
{
	[Foldout("Movement")]
	[SerializeField] List<float> speeds = new (2)
	{ 75, 35 };
	[EndFoldout]
	[SerializeField] float topSpeed = 15;
	[SerializeField] float moveDamping = 5;
	[SerializeField] float stopDamping = 15;

	[Header("Other")]

	[SerializeField] bool mouseMove;

	PlayerInput playerInput;
	InputManager inputs;
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

	void Awake()
	{
		playerInput = GetComponentInChildren<PlayerInput>();
		inputs = GetComponentInChildren<InputManager>();
		rb = GetComponentInChildren<Rigidbody2D>();
	}

	void Start() => Rebind(mouseMove);

	void Update()
	{
		if (!IsOwner || !IsSpawned) return;

		if (mouseMove) MouseMove();
		else Move();
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
		Vector3 mousePos = CameraMain.ScreenToWorldPoint(Input.mousePosition);
		Vector3 dir = (mousePos - transform.position).normalized;
		rb.AddForce(dir * speed);
	}
}
