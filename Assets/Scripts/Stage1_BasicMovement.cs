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
    public float jumpHeight = 6.5f;
    public float gravityScale = 1.5f;

    [Header("Input Actions")]
    public InputActionReference moveAction;
    public InputActionReference jumpAction;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip jumpSound;

    // Core components
    private Rigidbody2D r2d;
    private Transform t;
    private bool facingRight = true;
    private float moveDirection = 0;
    private Vector2 moveInput;
    private bool jumpPressed = false;

    void Start()
    {
        // Get references to components
        t = transform;
        r2d = GetComponent<Rigidbody2D>();

        // Configure rigidbody
        r2d.freezeRotation = true;
        r2d.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        r2d.gravityScale = gravityScale;

        // Set initial facing direction
        facingRight = t.localScale.x > 0;

        // Auto-find audio source if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // Enable input
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

        // Handle jumping - simple version, no ground check
        if (jumpPressed)
        {
            r2d.linearVelocity = new Vector2(r2d.linearVelocity.x, jumpHeight);

            // Play jump sound
            if (audioSource != null && jumpSound != null)
            {
                audioSource.PlayOneShot(jumpSound);
            }

            jumpPressed = false;
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

    // Input callback for jump action
    private void OnJump(InputAction.CallbackContext context)
    {
        jumpPressed = true;
    }
}