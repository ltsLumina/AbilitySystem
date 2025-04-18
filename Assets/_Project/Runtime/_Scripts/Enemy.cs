﻿#region
using UnityEngine;
#endregion

public abstract class Enemy : Entity
{
	protected override void OnTick() { }

	protected override void OnCycle() { }

	protected override void OnTriggerEnter2D(Collider2D other)
	{
		if (other.TryGetComponent(out Player player))
		{
			Debug.Log("Player entered enemy trigger.");
			player.OnHit(this);
		}
	}

	public override void TakeDamage(float damage)
	{
		base.TakeDamage(damage);

		Debug.Log($"{name} took {damage} damage.");

		var spriteRenderer = GetComponent<SpriteRenderer>();
		spriteRenderer.FlashSprite(Color.red, 0.5f);

		PopUpDamageNumbers.ShowDamage(damage, transform.position);
	}
}
