#region
using System.Diagnostics.CodeAnalysis;
using Lumina.Essentials.Attributes;
using UnityEngine;
#endregion

public abstract class Item : ScriptableObject
{
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public enum Rarity
	{
		Jade,
		Amber,
		Quartz,
		Sapphire,
		Ruby,
	}

	#region Rarity
	public (Rarity rarity, Color color) RarityColor => rarity switch
	{ Rarity.Jade     => (Rarity.Jade, new (0.42f, 0.96f, 0.67f)),
	  Rarity.Amber    => (Rarity.Amber, new (1f, 0.74f, 0.44f)),
	  Rarity.Quartz   => (Rarity.Quartz, new (0.85f, 0.85f, 0.95f)),
	  Rarity.Sapphire => (Rarity.Sapphire, new (0.1f, 0.55f, 0.86f)),
	  Rarity.Ruby     => (Rarity.Ruby, new Color(0.88f, 0.07f, 0.37f)),
	  _               => (Rarity.Jade, new Color(0.42f, 0.96f, 0.67f)) };
	#endregion

	[Header("Item Info")]

	[SerializeField] protected new string name;
	[TextArea(3, 5)]
	[SerializeField] protected string description;
	[Space(10)]
	[Header("Attributes")]

	[SerializeField] protected int damage;
	[SerializeField] protected float duration;
	[SerializeField] protected float cooldown;
	[SerializeField] protected StatusEffect buff;
	[Space(10)]
	[Header("Misc"), ReadOnly]
	[SerializeField] protected bool consumed;
	[SerializeField] protected bool invokeWhenAdded;

	public string Name => name;
	public string Description => description;
	public int Damage => damage;
	public float Duration => duration;
	public StatusEffect Buff => buff;

	[Space(10)]
	[SerializeField] Rarity rarity;

	public float Cooldown => cooldown;

	public bool Consumed
	{
		get => consumed;
		protected set => consumed = value;
	}
	public bool InvokeWhenAdded => invokeWhenAdded;

	public virtual void Action(Player owner) { }
}
