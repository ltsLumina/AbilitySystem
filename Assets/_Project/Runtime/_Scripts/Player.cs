using UnityEngine;

public class Player : Entity
{
    [SerializeField] int health = 10;
    
    protected override void OnMicroTick()
    {
        transform.position += Vector3.right * Time.deltaTime;
    }

    protected override void OnTick()
    {
        if (health <= 0) return;
        health--;
    }
}
