using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class CharacterController2D : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxSpeed = 3.4f;
    public float jumpHeight = 6.5f;
    public float gravityScale = 1.5f;
    [Range(0f, 1f)]
    public float airControlFactor = 0.5f; // How much control player has while in air (0 = no control, 1 = full control)

    [Header("Input Actions")]
    public InputActionReference moveAction;
    public InputActionReference jumpAction;

    bool facingRight = true;
    float moveDirection = 0;
    bool isGrounded = false;
    Rigidbody2D r2d;
    CapsuleCollider2D mainCollider;
    Transform t;

    // Input values
    private Vector2 moveInput;
    private bool jumpPressed = false;

    void Start()
    {
        t = transform;
        r2d = GetComponent<Rigidbody2D>();
        mainCollider = GetComponent<CapsuleCollider2D>();
        r2d.freezeRotation = true;
        r2d.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        r2d.gravityScale = gravityScale;
        facingRight = t.localScale.x > 0;

        // Enable input actions
        if (moveAction != null)
        {
            moveAction.action.Enable();
        }

        if (jumpAction != null)
        {
            jumpAction.action.Enable();
            jumpAction.action.performed += OnJump;
        }
    }

    void OnDestroy()
    {
        // Clean up input action callbacks
        if (jumpAction != null)
        {
            jumpAction.action.performed -= OnJump;
        }
    }

    void Update()
    {
        // Read input from the new Input System
        if (moveAction != null)
        {
            moveInput = moveAction.action.ReadValue<Vector2>();
        }

        // Movement controls
        if (Mathf.Abs(moveInput.x) > 0.1f)
        {
            moveDirection = moveInput.x;
        }
        else
        {
            if (isGrounded || r2d.linearVelocity.magnitude < 0.01f)
            {
                moveDirection = 0;
            }
        }

        // Change facing direction
        if (moveDirection != 0)
        {
            if (moveDirection > 0 && !facingRight)
            {
                facingRight = true;
                t.localScale = new Vector3(Mathf.Abs(t.localScale.x), t.localScale.y, transform.localScale.z);
            }
            if (moveDirection < 0 && facingRight)
            {
                facingRight = false;
                t.localScale = new Vector3(-Mathf.Abs(t.localScale.x), t.localScale.y, t.localScale.z);
            }
        }

        // Handle jumping
        if (jumpPressed && isGrounded)
        {
            r2d.linearVelocity = new Vector2(r2d.linearVelocity.x, jumpHeight);
            jumpPressed = false; // Reset jump flag
        }
    }

    void FixedUpdate()
    {
        Bounds colliderBounds = mainCollider.bounds;
        float colliderRadius = mainCollider.size.x * 0.4f * Mathf.Abs(transform.localScale.x);
        Vector3 groundCheckPos = colliderBounds.min + new Vector3(colliderBounds.size.x * 0.5f, colliderRadius * 0.9f, 0);

        // Check if player is grounded
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheckPos, colliderRadius);

        // Check if any of the overlapping colliders are not player collider, if so, set isGrounded to true
        isGrounded = false;
        if (colliders.Length > 0)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != mainCollider)
                {
                    isGrounded = true;
                    break;
                }
            }
        }

        // Apply movement velocity with air control
        float currentAirControl = isGrounded ? 1f : airControlFactor;
        float targetVelocityX = moveDirection * maxSpeed * currentAirControl;

        if (isGrounded)
        {
            // Full control when grounded
            r2d.linearVelocity = new Vector2(targetVelocityX, r2d.linearVelocity.y);
        }
        else
        {
            // Lerp towards target velocity when in air for smoother air control
            float newVelocityX = Mathf.Lerp(r2d.linearVelocity.x, targetVelocityX, Time.fixedDeltaTime * 10f);
            r2d.linearVelocity = new Vector2(newVelocityX, r2d.linearVelocity.y);
        }

        // Simple debug
        Debug.DrawLine(groundCheckPos, groundCheckPos - new Vector3(0, colliderRadius, 0), isGrounded ? Color.green : Color.red);
        Debug.DrawLine(groundCheckPos, groundCheckPos - new Vector3(colliderRadius, 0, 0), isGrounded ? Color.green : Color.red);
    }

    // Input callback for jump action
    private void OnJump(InputAction.CallbackContext context)
    {
        jumpPressed = true;
    }
}