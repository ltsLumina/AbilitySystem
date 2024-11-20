using System;
using System.Collections;
using DG.Tweening;
using Lumina.Essentials.Modules;
using UnityEngine;

public class Player : Entity
{
    [SerializeField] int health = 10;
    [SerializeField] float speed = 25;
    [SerializeField] float topSpeed = 15;
    [SerializeField] float moveDamping = 1;
    [SerializeField] float stopDamping = 15;

    [Header("Other")]
    [SerializeField] bool mouseMove;

    InputManager inputs;
    Rigidbody2D rb;

    protected override void OnMicroTick()
    {
        
    }

    protected override void OnTick()
    {
        if (health <= 0) return;
        health--;
    }

    protected override IEnumerator Start()
    {
        inputs = GetComponentInChildren<InputManager>();
        rb     = GetComponent<Rigidbody2D>();

        speed = mouseMove ? 35 : 25;
        
        return base.Start();
    }

    void OnValidate()
    {
        speed = mouseMove ? 35 : 25;
    }
    
    void Update()
    {
        if (mouseMove) MouseMove();
        else Move();
    }

    void Move()
    {
        rb.AddForce(inputs.MoveInput * speed);
        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, topSpeed);
            
        var changingDir = Vector2.Dot(rb.linearVelocity, inputs.MoveInput) < 0;
        rb.linearDamping = changingDir ? stopDamping : moveDamping;
        if (!inputs.IsMoving) rb.linearDamping = Mathf.Lerp(rb.linearDamping, 100, 0.1f);

        //rb.linearDamping = !inputs.IsMoving ? stopDamping : moveDamping;
    }

    void MouseMove()
    {
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var dir = (mousePos - transform.position).normalized;
        rb.AddForce(dir * speed);
    }
}
