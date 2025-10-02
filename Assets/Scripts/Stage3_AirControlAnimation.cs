using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class Stage3_AirControlAnimation : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxSpeed = 3.4f;
    public float jumpHeight = 6.5f;
    public float gravityScale = 1.5f;
    [Range(0f, 1f)]
    public float airControlFactor = 0.5f;

    [Header("Input Actions")]
    public InputActionReference moveAction;
    public InputActionReference jumpAction;

    [Header("Ground Detection")]
    public Collider2D groundCheckCollider;
    public LayerMask groundLayerMask = -1;

    [Header("Animation")]
    public Animator animator;

    // Core components
    private Rigidbody2D r2d;
    private Transform t;
    private bool facingRight = true;
    private float moveDirection = 0;
    private Vector2 moveInput;

    // Jump system
    private bool jumpPressed = false;
    private bool isGrounded = false;
    private bool wasGrounded = false;
    private int groundContactCount = 0;

    void Start()
    {
        t = transform;
        r2d = GetComponent<Rigidbody2D>();
        r2d.freezeRotation = true;
        r2d.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        r2d.gravityScale = gravityScale;
        facingRight = t.localScale.x > 0;

        // Auto-find animator if not assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

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
        if (jumpAction != null)
        {
            jumpAction.action.performed -= OnJump;
        }
    }

    void Update()
    {
        // Update ground state
        wasGrounded = isGrounded;
        isGrounded = groundContactCount > 0;

        // Read movement input
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

        // Flip character
        if (moveDirection > 0 && !facingRight)
        {
            Flip();
        }
        else if (moveDirection < 0 && facingRight)
        {
            Flip();
        }

        // Handle jumping
        if (jumpPressed && isGrounded && wasGrounded)
        {
            r2d.linearVelocity = new Vector2(r2d.linearVelocity.x, jumpHeight);
            jumpPressed = false;
        }

        if (!isGrounded)
        {
            jumpPressed = false;
        }

        // Update animator parameters
        if (animator != null)
        {
            float speed = Mathf.Abs(moveDirection);
            animator.SetFloat("Speed", speed);
            animator.SetBool("isGrounded", isGrounded);
        }
    }

    void FixedUpdate()
    {
        // Apply movement with air control factor
        float currentAirControl = isGrounded ? 1f : airControlFactor;
        float targetVelocityX = moveDirection * maxSpeed * currentAirControl;
        r2d.linearVelocity = new Vector2(targetVelocityX, r2d.linearVelocity.y);
    }

    void Flip()
    {
        facingRight = !facingRight;
        t.localScale = new Vector3(-t.localScale.x, t.localScale.y, t.localScale.z);
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        jumpPressed = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == groundCheckCollider) return;

        if (IsInLayerMask(other.gameObject.layer, groundLayerMask))
        {
            groundContactCount++;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other == groundCheckCollider) return;

        if (IsInLayerMask(other.gameObject.layer, groundLayerMask))
        {
            groundContactCount = Mathf.Max(0, groundContactCount - 1);
        }
    }

    private bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return layerMask == (layerMask | (1 << layer));
    }
}