using UnityEngine;

public class Enemy : Entity
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
}
