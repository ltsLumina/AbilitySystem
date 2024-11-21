#region
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VInspector;
using static Lumina.Essentials.Modules.Helpers;
using PaintDotNet.Data.PhotoshopFileType;
using UnityEngine.InputSystem;
#endregion


public class Player : Entity
{
    [SerializeField] int health = 10;
    [Foldout("Movement")]
    [SerializeField] float speed = 75;
    [SerializeField] float mouseSpeed = 35;
    [EndFoldout]
    [SerializeField] float topSpeed = 15;
    [SerializeField] float moveDamping = 5;
    [SerializeField] float stopDamping = 15;

    [Header("Other")]
    [SerializeField] bool mouseMove;

    InputManager inputs;
    Rigidbody2D rb;

    protected override void OnTick()
    {
        
    }

    protected override void OnCycle()
    {
        PerCycle(3, () =>
        {
            health = Mathf.Clamp(health - 1, 0, 10);
        });
    }

    void Reset() => gameObject.tag = "Player";

    protected override IEnumerator Start()
    {
        inputs = GetComponentInChildren<InputManager>();
        rb = GetComponent<Rigidbody2D>();
        
        return base.Start();
    }
    
    void FixedUpdate()
    {
        if (mouseMove) MouseMove();
        else Move();
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
        var mousePos = CameraMain.ScreenToWorldPoint(Input.mousePosition);
        var dir = (mousePos - transform.position).normalized;
        rb.AddForce(dir * mouseSpeed);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (!rb) return;
        Vector2 dir = rb.linearVelocity.normalized * 5;
        Gizmos.DrawRay(transform.position, dir);
        var point = transform.position + (Vector3) dir;
        Gizmos.DrawWireSphere(point, 0.3f);
    }
}
