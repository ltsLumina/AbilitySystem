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
using Random = UnityEngine.Random;
#endregion

public class Player : Entity, IPausable
{
	[SerializeField] Job job;
	
	[Space(10)]
	[Header("Health")]
	[SerializeField] int health = 3;
	[SerializeField] int maxHealth = 3;
	[SerializeField] float takeDamageCooldown = 1f;
	
	[Space(5)]
	[SerializeField] List<float> speeds = new (2)
	{ 50, 75 };
	[SerializeField] float topSpeed = 15;
	[SerializeField] float moveDamping = 5;
	[SerializeField] float stopDamping = 15;

	[Space(10)]
	[Header("Other")]
	[SerializeField] Color accentColour = new (0.43f, 0.5f, 0.49f);
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

	public Color AccentColour
	{
		get
		{
			accentColour.a = 1f;

			if (accentColour == Color.black) accentColour = Random.ColorHSV(0, 1, 1, 1, 1, 1, 1, 1);

			return accentColour;
		}
	}

	/// <summary>
	///     Called when the player takes damage.
	///     This event is triggered when the player takes damage, regardless of whether they have shields or not.
	///     If the player has shields, the event is triggered after the shields are removed.
	///     The boolean parameter indicates whether the player had shields or not when taking damage.
	/// </summary>
	public event Action<bool> OnTookDamage;
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

	[Button] [UsedImplicitly]
	void HurtPlayer() => TakeDamage(1);

	#region References
	public Job Job => job;
	public InputManager Inputs => GetComponentInChildren<InputManager>();
	public PlayerInput PlayerInput => Inputs.GetComponent<PlayerInput>();
	public Inventory Inventory { get; private set; }

	/// <summary>
	///     The ID of the player, which is equal to the player index + 1.
	/// </summary>
	public int ID => PlayerInput.playerIndex + 1;
	#endregion

	#region Properties
	public Stats Stats { get; private set; }
	#endregion

	protected override void OnTick() { }

	protected override void OnCycle() { }

	void Reset()
	{
		if (!Application.isPlaying) return;
		gameObject.tag = $"Player {ID}";
	}

	void Awake()
	{
		#region References
		Stats = GetComponent<Stats>();
		Inventory = GetComponent<Inventory>();
		#endregion

		gameObject.name = name;
	}

	new string name
	{
		get
		{
			string id = ID.ToString();
			string jobName = job.name;
			string playerName = $"Player {id} - {jobName}";
			return playerName;
		}
	}

	public override string ToString() => name;

	protected override void OnStart()
	{
		inputs = GetComponentInChildren<InputManager>();
		playerInput = inputs.GetComponent<PlayerInput>();
		rb = GetComponent<Rigidbody2D>();
		
		//Rebind(mouseMove);

		#region Init Player
		Health = maxHealth;

		OnDeath += () =>
		{
			Logger.LogWarning("Player has died!");

			statusEffects.Clear();

			//playerInput.DeactivateInput();
			//playerInput.SwitchCurrentActionMap("UI");

			//StopAllCoroutines();

			//SceneManagerExtended.ReloadScene();
		};

		GameManager.Instance.OnVictory += () => { statusEffects.Clear(); };
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

	void Update()
	{
		// Calculate screen bounds based on the camera's position and orthographic size
		Vector3 cameraPos = CameraMain.transform.position;
		float screenHalfHeight = CameraMain.orthographicSize;
		float screenHalfWidth = screenHalfHeight * CameraMain.aspect;

		// Clamp the player's position to the screen bounds
		Vector3 pos = transform.position;
		pos.x = Mathf.Clamp(pos.x, cameraPos.x - screenHalfWidth + 0.5f, cameraPos.x + screenHalfWidth - 0.5f);
		pos.y = Mathf.Clamp(pos.y, cameraPos.y - screenHalfHeight + 0.5f, cameraPos.y + screenHalfHeight - 0.5f);

		// Apply the clamped position
		transform.position = pos;
	}

	void FixedUpdate()
	{
		if (mouseMove) MouseMove();
		else Move();
	}

	void Move()
	{
		if (Vector2.Dot(rb.linearVelocity, inputs.MoveInput) < 0) rb.linearVelocity = Vector2.zero;
		
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
			OnTookDamage?.Invoke(true);

			DestroyNearbyOrbs();

			sprite.FlashSprite(Color.cyan, 0.3f);
			CameraMain.DOShakePosition(0.15f, 0.5f);

			StartCoroutine(DamageCooldown());
			return;
		}

		// Destroy all projectiles within a radius of 5 units
		DestroyNearbyOrbs();

		Debug.Assert((int) damage == 1, $"Tried to deal {damage} damage to {name}, but it was clamped to {(int) damage}." + "\nThis is likely due to the damage being negative, zero, or greater than the current health.");
		Health -= Mathf.RoundToInt(damage);
		OnTookDamage?.Invoke(false);
		StartCoroutine(DamageCooldown());

		base.TakeDamage(damage); // logs the damage taken

		sprite.FlashSprite(Color.red, 0.3f);
		CameraMain.DOShakePosition(0.3f, 1f);

		return;
		void DestroyNearbyOrbs()
		{
			foreach (Collider2D col in Physics2D.OverlapCircleAll(transform.position, 5))
			{
				if (col.CompareTag("Projectile"))
					Destroy(col.gameObject);
			}
		}
	}

	bool isOnCooldown;

	IEnumerator DamageCooldown()
	{
		isOnCooldown = true;
		yield return new WaitForSeconds(takeDamageCooldown); 
		isOnCooldown = false;
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
		Vector3 screenPosition = CameraMain.WorldToScreenPoint(transform.position + Vector3.up * 2); // Offset above the player's head
		screenPosition.y = Screen.height - screenPosition.y;                                         // Convert to GUI coordinates

		GUILayout.BeginArea(new (screenPosition.x - 50, screenPosition.y - 20, 200, 50));

		foreach (StatusEffect effect in statusEffects)
		{
			if (effect == null) continue;

			GUILayout.Label($"{effect.StatusName} - {effect.Time.RoundTo(1)}");
		}

		GUILayout.EndArea();
	}

	public void Pause() => enabled = false;

	public void Resume() => enabled = true;
}
