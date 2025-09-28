using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Enemy : MonoBehaviour
{
    [Header("AI Settings")]
    public float sightRange = 5f;
    public float dashSpeed = 8f;
    public LayerMask playerLayerMask = -1;
    public float timeToSubtract = 3f;
    public float timeToAdd = 5f;
    public float bounceForce = 5f;
    public float bounceCooldown = 1f;

    [Header("Animation")]
    public Animator animator;

    private enum EnemyState
    {
        Idle,
        Dashing
    }

    private EnemyState currentState = EnemyState.Idle;
    private Transform player;
    private Rigidbody2D rb;
    private bool facingRight = true;
    private float lastBounceTime = 0f;

    private float subtractTimeCooldownTimer = 1f;
    private float cooldownDuration = 1f;
    private bool hasAddedTime = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        // Find the player
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // Auto-find animator if not assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    void Update()
    {
        if (player == null) return;

        subtractTimeCooldownTimer += Time.deltaTime;

        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdleState();
                break;
            case EnemyState.Dashing:
                HandleDashState();
                break;
        }

        // Update animations
        UpdateAnimations();
    }

    void HandleIdleState()
    {
        // Stop movement
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        // Check if player is in sight range (only if not recently bounced)
        if (Time.time >= lastBounceTime + bounceCooldown)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer <= sightRange)
            {
                StartDash();
            }
        }
    }

    void HandleDashState()
    {
        // Keep dashing toward player's current position
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(directionToPlayer.x * dashSpeed, rb.linearVelocity.y);

        // Face the direction we're moving
        if (directionToPlayer.x > 0 && !facingRight)
        {
            Flip();
        }
        else if (directionToPlayer.x < 0 && facingRight)
        {
            Flip();
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }

    void StartDash()
    {
        currentState = EnemyState.Dashing;
    }

    void UpdateAnimations()
    {
        if (animator == null) return;

        // Set animation parameters based on current state
        animator.SetBool("IsIdle", currentState == EnemyState.Idle);
        animator.SetBool("IsDashing", currentState == EnemyState.Dashing);
    }

    // Called by player's attack system - instantly kills enemy
    public void Die()
    {
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // Destroy after short delay for death animation
        StartCoroutine(DestroyAfterDelay(0.5f));
    }

    public float GetTimeToAdd()
    {
        if (hasAddedTime)
        {
            return 0;
        }
        else
        {
            hasAddedTime = true;
            return timeToAdd;
        }
    }

    IEnumerator DestroyAfterDelay(float delay)
    {
        // Stop movement during death animation
        rb.linearVelocity = Vector2.zero;
        currentState = EnemyState.Idle; // Prevent further state changes

        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    // Subtract time on contact
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Subtract time from timer
            if (subtractTimeCooldownTimer > cooldownDuration)
            {
                Timer.OnSubtractTime(timeToSubtract);
                subtractTimeCooldownTimer = 0f;   
            }

            // Calculate bounce direction (away from player)
            Vector2 bounceDirection = (transform.position - collision.transform.position).normalized;

            // Method 1: Direct translation (instant movement)
            transform.Translate(bounceDirection * bounceForce * 0.01f);

            // Method 2: Set velocity directly (alternative)
            // rb.linearVelocity = new Vector2(bounceDirection.x * bounceForce, 0);

            // Method 3: Stop dash movement and apply force (alternative)
            // rb.linearVelocity = Vector2.zero;
            // rb.AddForce(bounceDirection * bounceForce, ForceMode2D.Impulse);

            // Return to idle state and set bounce cooldown
            currentState = EnemyState.Idle;
            lastBounceTime = Time.time;

            Debug.Log($"{name} bounced off player! Direction: {bounceDirection}, Force: {bounceForce}");
        }
    }

    // Debug visualization
    void OnDrawGizmosSelected()
    {
        // Draw sight range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}