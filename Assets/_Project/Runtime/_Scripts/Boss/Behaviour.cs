#region
using System;
using JetBrains.Annotations;
using UnityEngine;
using VInspector;
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
	}

	[UsedImplicitly]
	[SerializeField] public string description;
	[SerializeField] public Type type;
	[ShowIf(nameof(type), Type.Move)]
	[SerializeField] protected Vector3 position;
	[ShowIf(nameof(type), Type.Attack)]
	[SerializeField] protected string attack;
	[ShowIf(nameof(type), Type.Dialogue)]
	[SerializeField] protected string dialogue;
	[EndIf]

	// ReSharper disable once ParameterHidesMember
	public void Start(Entity context)
	{
		switch (type)
		{
			case Type.Move:
				new Move().Invoke(context);
				break;

			case Type.Attack:
				new Attack().Invoke(context);
				break;

			case Type.Dialogue:
				new Dialogue().Invoke(context);
				break;

			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	protected virtual void Invoke(Entity context) { }
}

[Serializable]
public class Move : Behaviour
{
	protected override void Invoke(Entity context)
	{
		context.transform.position = position;
		Debug.Log($"Moved to position {position}!");
	}
}

public class Attack : Behaviour
{
	protected override void Invoke(Entity context) => Debug.Log("Attacked.");
}

public class Dialogue : Behaviour
{
	protected override void Invoke(Entity context) => Debug.Log("Pretend I'm speaking");
}

// propertydrawer for Behaviour
