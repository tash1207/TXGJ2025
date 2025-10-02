using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class Stage5_CompleteController : MonoBehaviour
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
    public InputActionReference attackAction;

    [Header("Ground Detection")]
    public Collider2D groundCheckCollider;
    public LayerMask groundLayerMask = -1;

    [Header("Attack System")]
    public ParticleSystem sprayAttack;
    public float attackDuration = 0.5f;
    public float attackCooldown = 0.3f;
    public LayerMask enemyLayerMask = -1;
    public AudioSource audioSource;
    public AudioClip attackSound;

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

    // Attack system
    private bool attackPressed = false;
    private float lastAttackTime = 0f;

    // Game state
    private bool allowMovement = true;
    public bool IsDead = false;

    void Start()
    {
        t = transform;
        r2d = GetComponent<Rigidbody2D>();
        r2d.freezeRotation = true;
        r2d.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        r2d.gravityScale = gravityScale;
        facingRight = t.localScale.x > 0;

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
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
        // Subscribe to game events
        Timer.OnTimeRunOut += Die;
        WinGame.OnGameWon += PausePlayerMovement;
    }

    void OnDisable()
    {
        // Unsubscribe from game events
        Timer.OnTimeRunOut -= Die;
        WinGame.OnGameWon -= PausePlayerMovement;
    }

    void OnDestroy()
    {
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
        // Check if movement is allowed
        if (!allowMovement) { return; }

        wasGrounded = isGrounded;
        isGrounded = groundContactCount > 0;

        if (moveAction != null)
        {
            moveInput = moveAction.action.ReadValue<Vector2>();
        }

        if (Mathf.Abs(moveInput.x) > 0.1f)
        {
            moveDirection = moveInput.x;
        }
        else
        {
            moveDirection = 0;
        }

        if (moveDirection > 0 && !facingRight)
        {
            Flip();
        }
        else if (moveDirection < 0 && facingRight)
        {
            Flip();
        }

        if (jumpPressed && isGrounded && wasGrounded)
        {
            r2d.linearVelocity = new Vector2(r2d.linearVelocity.x, jumpHeight);
            jumpPressed = false;
        }

        if (!isGrounded)
        {
            jumpPressed = false;
        }

        if (attackPressed && Time.time >= lastAttackTime + attackCooldown)
        {
            PerformAttack();
            attackPressed = false;
            lastAttackTime = Time.time;
        }

        if (animator != null)
        {
            float speed = Mathf.Abs(moveDirection);
            animator.SetFloat("Speed", speed);
            animator.SetBool("isGrounded", isGrounded);
        }
    }

    void FixedUpdate()
    {
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

    private void OnAttack(InputAction.CallbackContext context)
    {
        attackPressed = true;
    }

    private void PerformAttack()
    {
        if (sprayAttack == null) return;

        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }

        Vector3 attackPosition = transform.position + new Vector3(facingRight ? 0.5f : -0.5f, 0, 0);
        sprayAttack.transform.position = attackPosition;

        if (facingRight)
        {
            sprayAttack.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            sprayAttack.transform.rotation = Quaternion.Euler(0, 180, 0);
        }

        sprayAttack.Play();
        StartCoroutine(AttackCoroutine());
    }

    private IEnumerator AttackCoroutine()
    {
        float attackTimer = 0f;

        while (attackTimer < attackDuration)
        {
            Vector2 attackDirection = facingRight ? Vector2.right : Vector2.left;
            Vector2 attackOrigin = (Vector2)transform.position + attackDirection * 0.5f;

            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackOrigin, 1.5f, enemyLayerMask);

            foreach (Collider2D enemy in hitEnemies)
            {
                Vector2 toEnemy = (enemy.transform.position - transform.position).normalized;
                float dotProduct = Vector2.Dot(attackDirection, toEnemy);

                if (dotProduct > 0.3f)
                {
                    var enemyScript = enemy.GetComponent<Enemy>();
                    if (enemyScript != null)
                    {
                        float timeToAdd = enemyScript.GetTimeToAdd();
                        enemyScript.Die();

                        var timer = FindObjectOfType<Timer>();
                        if (timer != null)
                        {
                            timer.AddTime(timeToAdd);
                        }
                    }
                }
            }

            attackTimer += Time.deltaTime;
            yield return null;
        }
    }

    // Called when player dies (from Timer event)
    public void Die()
    {
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        PausePlayerMovement();
        IsDead = true;
    }

    // Stops player movement (for death or win condition)
    void PausePlayerMovement()
    {
        allowMovement = false;
        r2d.linearVelocity = Vector2.zero;
        moveDirection = 0;

        if (animator != null)
        {
            animator.SetFloat("Speed", 0);
        }
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