using UnityEngine;
using UnityEngine.InputSystem;

// This script makes a 2D character move left/right and jump
public class Stage1_BasicMovement : MonoBehaviour
{
    // === MOVEMENT SETTINGS (you can change these in the Inspector) ===
    public float moveSpeed = 5f;      // How fast the player moves
    public float jumpForce = 10f;     // How high the player jumps

    // === INPUT ACTIONS (drag your Input Actions here in the Inspector) ===
    public InputActionReference moveAction;
    public InputActionReference jumpAction;

    // === PRIVATE VARIABLES (the script uses these internally) ===
    private Rigidbody2D rb;           // Reference to the Rigidbody2D component
    private bool facingRight = true;  // Which direction is the player facing?
    private float moveInput = 0;      // Stores the current movement input
    private bool jumpPressed = false; // Has the jump button been pressed?

    // Start runs once when the game begins
    void Start()
    {
        // Get the Rigidbody2D component attached to this game object
        rb = GetComponent<Rigidbody2D>();

        // Stop the player from rotating when they move
        rb.freezeRotation = true;

        // Enable the input actions so they can receive input
        if (moveAction != null)
        {
            moveAction.action.Enable();
        }

        if (jumpAction != null)
        {
            jumpAction.action.Enable();
            jumpAction.action.performed += OnJump; // Call OnJump when jump button is pressed
        }
    }

    // OnDestroy runs when the object is destroyed
    void OnDestroy()
    {
        // Clean up the jump action callback
        if (jumpAction != null)
        {
            jumpAction.action.performed -= OnJump;
        }
    }

    // Update runs every frame
    void Update()
    {
        // === READ MOVEMENT INPUT ===
        if (moveAction != null)
        {
            Vector2 input = moveAction.action.ReadValue<Vector2>();
            moveInput = input.x; // Get the horizontal (left/right) input
        }

        // === MOVE THE PLAYER ===
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // === FLIP CHARACTER TO FACE MOVEMENT DIRECTION ===
        // If moving right and facing left, flip to face right
        if (moveInput > 0 && !facingRight)
        {
            Flip();
        }
        // If moving left and facing right, flip to face left
        else if (moveInput < 0 && facingRight)
        {
            Flip();
        }

        // === HANDLE JUMPING ===
        if (jumpPressed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpPressed = false; // Reset the jump flag
        }
    }

    // This function flips the character to face the other direction
    void Flip()
    {
        facingRight = !facingRight; // Toggle the facing direction

        // Flip the character by reversing the X scale
        Vector3 scale = transform.localScale;
        scale.x *= -1; // Multiply X by -1 to flip
        transform.localScale = scale;
    }

    // This function is called when the jump button is pressed
    private void OnJump(InputAction.CallbackContext context)
    {
        jumpPressed = true;
    }
}