#region
using System;
using System.Collections;
using System.Reflection;
using DG.Tweening;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using VInspector;
using Object = UnityEngine.Object;
#endregion

/// <summary>
///     The base class for a boss behaviour.
///     A sequence of behaviours is a phase.
///     Thereby, a sequence of phases is a stage, AKA the boss fight.
///     <example> Behaviour 1 (Move) -> Behaviour 2 (Attack) -> Behaviour 3 (Recover) -> Repeat </example>
/// </summary>
[Serializable]
public class Behaviour
{
	public enum Type
	{
		Move,
		Attack,
		Dialogue,
		Wait,
	}

	[UsedImplicitly]
	[SerializeField] public string description;
	[SerializeField] public Type type;
	[SerializeField] protected Vector2 position;
	[SerializeField] protected string attackKey;
	[SerializeField] protected bool showWarning;
	[SerializeField] protected string dialogue;
	[EndIf]
	[Space(5)]
	[Tooltip("The duration of the behaviour, i.e., how long before the next behaviour starts")]
	[SerializeField] protected float duration;
	[Tooltip("The delay before the attack completes. A Donut AoE will wait for this time before spawning orbs")]
	[SerializeField] protected float delay;

	public float Duration
	{
		get => duration;
		private set => duration = value;
	}

	// ReSharper disable once ParameterHidesMember
	public void Start(Entity context)
	{
		switch (type)
		{
			case Type.Move:
				new Move(position, duration).Invoke(context);
				break;

			case Type.Attack:
				new Attack(position, attackKey, showWarning, duration, delay).Invoke(context);
				break;

			case Type.Dialogue:
				new Dialogue(dialogue, duration).Invoke(context);
				break;

			case Type.Wait:
				new Wait(duration).Invoke(context);
				break;

			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	protected virtual void Invoke(Entity self) { }
}

public class Move : Behaviour
{
	public Move(Vector2 position, float duration)
	{
		this.position = position;
		this.duration = duration;
	}

	protected override void Invoke(Entity self) => self.transform.DOMove(position.WithStageOffset(), duration).SetEase(Ease.OutCubic).SetLink(self.gameObject).SetId("Move");
}

public class Attack : Behaviour
{
	public Attack(Vector2 position, string attackKey, bool showWarning, float duration, float delay)
	{
		this.position = position;
		this.attackKey = attackKey;
		this.showWarning = showWarning;
		this.delay = delay;
		this.duration = duration;
	}

	protected override void Invoke(Entity self)
	{
		string key = attackKey;

		var attacks = self.gameObject.GetComponent<Attacks>();
		MethodInfo method = typeof(Attacks).GetMethod(key);

		if (method != null)
		{
			ParameterInfo[] parameters = method.GetParameters();

			object[] args = parameters.Length > 0
					? new object[]
					{ position.WithStageOffset(), showWarning, delay }
					: null;

			method.Invoke(attacks, args);
		}
		else Debug.LogError($"Attack method \"{key}\" not found in {attacks}. \nPlease check the key in the Behaviour Inspector.");
	}
}

public class Dialogue : Behaviour
{
	public Dialogue(string dialogue, float duration)
	{
		this.dialogue = dialogue;
		this.duration = duration;
	}

	protected override void Invoke(Entity self) => self.StartCoroutine(ShowText(self));

	IEnumerator ShowText(Entity self)
	{
		var dialogueTextPrefab = GameObject.Find("Dialogue Text").GetComponent<TextMeshProUGUI>();
		TextMeshProUGUI dialogueText = Object.Instantiate(dialogueTextPrefab, self.transform.position, Quaternion.identity, GameObject.FindWithTag("Worldspace Canvas").transform);

		Vector3 offset = Vector3.up * 1.5f;

		dialogueText.text = dialogue;
		dialogueText.enabled = true;

		dialogueText.transform.position = self.transform.position + offset;
		Tween moveTween = dialogueText.transform.DOMove(self.transform.position + offset, duration).SetEase(Ease.Linear).SetLink(self.gameObject).OnUpdate(() => dialogueText.transform.position = self.transform.position + offset);

		yield return new WaitForSeconds(duration);

		moveTween.Kill();
		Object.Destroy(dialogueText.gameObject);
	}
}

public class Wait : Behaviour
{
	public Wait(float duration) { this.duration = duration; }

	protected override void Invoke(Entity self) => self.StartCoroutine(WaitFor(duration));

	IEnumerator WaitFor(float duration) { yield return new WaitForSeconds(duration); }
}
