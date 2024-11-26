#region
using UnityEngine;
#endregion

public class Enemy : Entity, IDamageable
{
    protected override void OnTick() { }

    protected override void OnCycle() { }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Player player))
        {
            Debug.Log("Player entered enemy trigger.");
            player.OnHit(this);
        }
    }

    public void TakeDamage(float damage, params StatusEffect[] statusEffects)
    {
        Debug.Log($"{name} took {damage} damage.");

        var spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.FlashSprite(Color.red, 0.5f);

        PopUpDamageNumbers.ShowDamage(damage, transform.position);
    }
}
