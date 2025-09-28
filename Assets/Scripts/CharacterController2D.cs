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

    [Header("Ground Detection")]
    public Collider2D groundCheckCollider;
    public LayerMask groundLayerMask = -1;

    bool facingRight = true;
    float moveDirection = 0;
    bool isGrounded = false;
    Rigidbody2D r2d;
    CapsuleCollider2D mainCollider;
    Transform t;

    // Input values
    private Vector2 moveInput;
    private bool jumpPressed = false;
    private bool wasGrounded = false;
    private int groundContactCount = 0;

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
        // Store previous ground state
        wasGrounded = isGrounded;

        // Update ground state based on trigger contacts
        isGrounded = groundContactCount > 0;

        // Read input from the new Input System
        if (moveAction != null)
        {
            moveInput = moveAction.action.ReadValue<Vector2>();
        }

        // Movement controls - immediate response for precise air control
        if (Mathf.Abs(moveInput.x) > 0.1f)
        {
            moveDirection = moveInput.x;
        }
        else
        {
            // Stop immediately when input is released (Megaman-style)
            moveDirection = 0;
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
        if (jumpPressed && isGrounded && wasGrounded)
        {
            r2d.linearVelocity = new Vector2(r2d.linearVelocity.x, jumpHeight);
            jumpPressed = false; // Reset jump flag
        }

        // Clear jump input if we're not grounded to prevent buffering
        if (!isGrounded)
        {
            jumpPressed = false;
        }

        // Clear jump input if we're not grounded to prevent buffering
        if (!isGrounded)
        {
            jumpPressed = false;
        }
    }

    void FixedUpdate()
    {
        // Apply movement velocity with precise air control (Megaman-style)
        float currentAirControl = isGrounded ? 1f : airControlFactor;
        float targetVelocityX = moveDirection * maxSpeed * currentAirControl;

        // Direct velocity assignment for immediate response (like Megaman)
        r2d.linearVelocity = new Vector2(targetVelocityX, r2d.linearVelocity.y);
    }

    // Trigger events for ground detection
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == groundCheckCollider) return; // Ignore self

        // Check if the colliding object is on the ground layer
        if (IsInLayerMask(other.gameObject.layer, groundLayerMask))
        {
            groundContactCount++;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other == groundCheckCollider) return; // Ignore self

        // Check if the colliding object is on the ground layer
        if (IsInLayerMask(other.gameObject.layer, groundLayerMask))
        {
            groundContactCount = Mathf.Max(0, groundContactCount - 1);
        }
    }

    // Helper function to check if a layer is in the layer mask
    private bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return layerMask == (layerMask | (1 << layer));
    }

    // Input callback for jump action
    private void OnJump(InputAction.CallbackContext context)
    {
        jumpPressed = true;
    }
}