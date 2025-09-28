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
    public InputActionReference attackAction;

    [Header("Ground Detection")]
    public Collider2D groundCheckCollider;
    public LayerMask groundLayerMask = -1;

    [Header("Attack System")]
    public ParticleSystem sprayAttack;
    public float attackDuration = 0.5f;
    public float attackCooldown = 0.3f;
    public LayerMask enemyLayerMask = -1;

    [Header("Animation")]
    public Animator animator;

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
    private bool attackPressed = false;
    private float lastAttackTime = 0f;

    bool allowMovement = true;

    void Start()
    {
        t = transform;
        r2d = GetComponent<Rigidbody2D>();
        mainCollider = GetComponent<CapsuleCollider2D>();
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

        if (attackAction != null)
        {
            attackAction.action.Enable();
            attackAction.action.performed += OnAttack;
        }
    }

    void OnEnable()
    {
        Timer.OnTimeRunOut += Die;
    }

    void OnDisable()
    {
        Timer.OnTimeRunOut -= Die;
    }

    void OnDestroy()
    {
        // Clean up input action callbacks
        if (jumpAction != null)
        {
            jumpAction.action.performed -= OnJump;
        }

        if (attackAction != null)
        {
            attackAction.action.performed -= OnAttack;
        }
    }

    void Update()
    {
        if (!allowMovement) { return; }

        // Store previous ground state
        wasGrounded = isGrounded;

        // Update ground state based on trigger contacts
        isGrounded = groundContactCount > 0;

        // Debug ground state
        Debug.Log($"Ground contacts: {groundContactCount}, isGrounded: {isGrounded}");

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

        // Handle attacking
        if (attackPressed && Time.time >= lastAttackTime + attackCooldown)
        {
            PerformAttack();
            attackPressed = false;
            lastAttackTime = Time.time;
        }

        // Update animator parameters
        if (animator != null)
        {
            float speed = Mathf.Abs(moveDirection);
            animator.SetFloat("Speed", speed);
            animator.SetBool("isGrounded", isGrounded); // Uncommented - make sure you have this parameter in your Animator

            // Debug animation values (remove this line once animations are working)
            Debug.Log($"Speed: {speed}, isGrounded: {isGrounded}, moveDirection: {moveDirection}");
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
        Debug.Log($"Trigger Enter: {other.name}, Layer: {other.gameObject.layer}");

        if (other == groundCheckCollider) return; // Ignore self

        // Check if the colliding object is on the ground layer
        if (IsInLayerMask(other.gameObject.layer, groundLayerMask))
        {
            groundContactCount++;
            Debug.Log($"Ground contact added! Total: {groundContactCount}");
        }
        else
        {
            Debug.Log($"Not ground layer. Expected layers in mask: {groundLayerMask.value}");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log($"Trigger Exit: {other.name}, Layer: {other.gameObject.layer}");

        if (other == groundCheckCollider) return; // Ignore self

        // Check if the colliding object is on the ground layer
        if (IsInLayerMask(other.gameObject.layer, groundLayerMask))
        {
            groundContactCount = Mathf.Max(0, groundContactCount - 1);
            Debug.Log($"Ground contact removed! Total: {groundContactCount}");
        }
        else
        {
            Debug.Log($"Exit from non-ground layer object: {other.name}");
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

    // Input callback for attack action
    private void OnAttack(InputAction.CallbackContext context)
    {
        attackPressed = true;
    }

    private void PerformAttack()
    {
        if (sprayAttack == null) return;

        // Position the particle system in front of the player
        Vector3 attackPosition = transform.position + new Vector3(facingRight ? 0.5f : -0.5f, 0, 0);
        sprayAttack.transform.position = attackPosition;

        // Rotate the entire particle system based on facing direction
        if (facingRight)
        {
            sprayAttack.transform.rotation = Quaternion.Euler(0, 0, 0); // Face right
        }
        else
        {
            sprayAttack.transform.rotation = Quaternion.Euler(0, 180, 0); // Face left (flip Y axis)
        }

        // Alternative method if the above doesn't work - modify the shape module
        var shape = sprayAttack.shape;
        if (facingRight)
        {
            shape.rotation = new Vector3(0, 0, 0);
        }
        else
        {
            shape.rotation = new Vector3(0, 0, 180);
        }

        // Play the particle system
        sprayAttack.Play();

        // Start coroutine to detect enemies in spray area
        StartCoroutine(AttackCoroutine());

        Debug.Log($"Attack performed facing {(facingRight ? "right" : "left")}!");
    }

    private IEnumerator AttackCoroutine()
    {
        float attackTimer = 0f;

        while (attackTimer < attackDuration)
        {
            // Create a cone area in front of the player for enemy detection
            Vector2 attackDirection = facingRight ? Vector2.right : Vector2.left;
            Vector2 attackOrigin = (Vector2)transform.position + attackDirection * 0.5f;

            // Check for enemies in attack range
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackOrigin, 1.5f, enemyLayerMask);

            foreach (Collider2D enemy in hitEnemies)
            {
                // Check if enemy is in front of player (cone check)
                Vector2 toEnemy = (enemy.transform.position - transform.position).normalized;
                float dotProduct = Vector2.Dot(attackDirection, toEnemy);

                if (dotProduct > 0.3f) // 60-degree cone
                {
                    // Try to get enemy script and damage/kill it
                    var enemyScript = enemy.GetComponent<Enemy>();
                    if (enemyScript != null)
                    {
                        enemyScript.Die();
                        Debug.Log($"Killed enemy: {enemy.name}");

                        // Add time to timer (assuming you have a timer script)
                        var timer = FindObjectOfType<Timer>();
                        if (timer != null)
                        {
                            timer.AddTime(5f); // Add 5 seconds for killing an enemy
                        }
                    }
                }
            }

            attackTimer += Time.deltaTime;
            yield return null;
        }
    }

    // Call this method when player dies
    public void Die()
    {
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        allowMovement = false;
        r2d.linearVelocity = Vector2.zero;
        moveDirection = 0;
        animator.SetFloat("Speed", 0);

        Debug.Log("Player died!");
    }
}