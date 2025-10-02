using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class Stage1_BasicMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxSpeed = 3.4f;

    [Header("Input Actions")]
    public InputActionReference moveAction;

    // Core components
    private Rigidbody2D r2d;
    private Transform t;
    private bool facingRight = true;
    private float moveDirection = 0;
    private Vector2 moveInput;

    void Start()
    {
        // Get references to components
        t = transform;
        r2d = GetComponent<Rigidbody2D>();

        // Configure rigidbody
        r2d.freezeRotation = true;
        r2d.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Set initial facing direction
        facingRight = t.localScale.x > 0;

        // Enable input
        if (moveAction != null)
        {
            moveAction.action.Enable();
        }
    }

    void Update()
    {
        // Read input from the new Input System
        if (moveAction != null)
        {
            moveInput = moveAction.action.ReadValue<Vector2>();
        }

        // Get movement direction
        if (Mathf.Abs(moveInput.x) > 0.1f)
        {
            moveDirection = moveInput.x;
        }
        else
        {
            moveDirection = 0;
        }

        // Flip character to face movement direction
        if (moveDirection > 0 && !facingRight)
        {
            Flip();
        }
        else if (moveDirection < 0 && facingRight)
        {
            Flip();
        }
    }

    void FixedUpdate()
    {
        // Apply movement velocity
        float targetVelocityX = moveDirection * maxSpeed;
        r2d.linearVelocity = new Vector2(targetVelocityX, r2d.linearVelocity.y);
    }

    void Flip()
    {
        facingRight = !facingRight;
        t.localScale = new Vector3(-t.localScale.x, t.localScale.y, t.localScale.z);
    }
}