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

    private enum EnemyState
    {
        Idle,
        Dashing
    }

    private EnemyState currentState = EnemyState.Idle;
    private Transform player;
    private Rigidbody2D rb;
    private bool facingRight = true;

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
    }

    void Update()
    {
        if (player == null) return;

        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdleState();
                break;
            case EnemyState.Dashing:
                HandleDashState();
                break;
        }
    }

    void HandleIdleState()
    {
        // Stop movement
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        // Check if player is in sight range
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= sightRange)
        {
            StartDash();
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
        Debug.Log($"{name} is dashing at player!");
    }

    // Called by player's attack system - instantly kills enemy
    public void Die()
    {
        Debug.Log($"{name} was killed by player attack!");
        Destroy(gameObject);
    }

    // Instantly kill player on contact
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            var playerScript = collision.gameObject.GetComponent<CharacterController2D>();
            if (playerScript != null)
            {
                playerScript.Die();
                Debug.Log($"{name} killed the player!");
            }
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