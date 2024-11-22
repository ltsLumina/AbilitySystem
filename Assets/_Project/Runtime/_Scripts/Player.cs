#region
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using VInspector;
using static Lumina.Essentials.Modules.Helpers;
#endregion

public class Player : Entity, IDamageable
{
    [HideInInspector, UsedImplicitly]
    public VInspectorData data;

    [SerializeField] int health = 10;
    [Foldout("Movement")]
    [SerializeField] List<float> speeds = new (2)
    { 75, 35 };
    [EndFoldout]
    [SerializeField] float topSpeed = 15;
    [SerializeField] float moveDamping = 5;
    [SerializeField] float stopDamping = 15;

    [Header("Other")]
    [SerializeField] bool mouseMove;

    InputManager inputs;
    Rigidbody2D rb;

    float speed
    {
        get => mouseMove ? speeds[1] : speeds[0];
        set
        {
            if (mouseMove) speeds[1] = value;
            else speeds[0] = value;
        }
    }

    protected override void OnTick() { }

    protected override void OnCycle() { PerCycle(3, () => { health = Mathf.Clamp(health - 1, 0, 10); }); }

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
        rb.AddForce(dir * speed);
    }

    public void OnHit(Enemy enemy = default)
    {
        health--;

        var sprite = GetComponentInChildren<SpriteRenderer>();
        sprite.FlashSprite(Color.red, 0.3f);
        StartCoroutine(sprite.CreateAfterImages(0.05f, 0.25f, 5));

        StartCoroutine(Foo());
    }

    IEnumerator Foo()
    {
        speed *= 1.5f;
        yield return new WaitForSeconds(0.5f);
        speed /= 1.5f;
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

    public void TakeDamage(float damage)
    {
        health -= Mathf.RoundToInt(damage);
        if (health <= 0) Logger.Log("Player has died.");
    }
}
