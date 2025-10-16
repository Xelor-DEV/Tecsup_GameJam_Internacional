using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float acceleration = 15f;
    [SerializeField] private float deceleration = 20f;
    [SerializeField] private float velocityPower = 0.9f;

    [Header("Smoothing")]
    [SerializeField] private float movementSmoothing = 0.1f;

    [SerializeField] Rigidbody2D rb;
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer spriteRenderer;
    private Vector2 currentVelocity;
    private Vector2 targetVelocity;
    private Vector2 smoothVelocity;

    private Vector2 moveInput;
    private bool isRunning = false;
    private float currentSpeed;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        //animator = GetComponent<Animator>();
        //spriteRenderer = GetComponent<SpriteRenderer>();
        currentSpeed = walkSpeed;

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearDamping = 0f;
            rb.angularDamping = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    void FixedUpdate()
    {
        HandleMovement();
        UpdateAnimations();
        HandleSpriteFlip();
    }

    public void GetMovement(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        moveInput = input.normalized;
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.started || context.performed)
        {
            isRunning = true;
            currentSpeed = runSpeed;
        }
        else if (context.canceled)
        {
            isRunning = false;
            currentSpeed = walkSpeed;
        }
    }

    void HandleMovement()
    {
        // Calculate target velocity
        targetVelocity = moveInput * currentSpeed;

        // Apply acceleration and deceleration
        Vector2 velocityDifference = targetVelocity - rb.linearVelocity;
        float accelerationRate = (targetVelocity.magnitude > 0.01f) ? acceleration : deceleration;

        float speedDifferenceMagnitude = velocityDifference.magnitude;
        float movementForceMagnitude = Mathf.Pow(speedDifferenceMagnitude, velocityPower) * accelerationRate;
        Vector2 movementForce = velocityDifference.normalized * movementForceMagnitude;

        rb.AddForce(movementForce, ForceMode2D.Force);

        // Additional movement smoothing
        rb.linearVelocity = Vector2.SmoothDamp(rb.linearVelocity, targetVelocity, ref smoothVelocity, movementSmoothing);

        // Clamp velocity if exceeds current speed
        if (rb.linearVelocity.magnitude > currentSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * currentSpeed;
        }
    }

    void HandleSpriteFlip()
    {
        // Voltear el sprite según la dirección horizontal
        if (moveInput.x > 0.1f) // Derecha
        {
            spriteRenderer.flipX = false;
        }
        else if (moveInput.x < -0.1f) // Izquierda
        {
            spriteRenderer.flipX = true;
        }
    }

    void UpdateAnimations()
    {
        if (animator != null)
        {
            bool isMoving = IsMoving();

            animator.SetBool("IsMoving", isMoving);
            animator.SetBool("IsRunning", IsRunning());
        }
    }

    public Vector2 GetMovementDirection()
    {
        return moveInput;
    }

    public bool IsMoving()
    {
        return moveInput.magnitude > 0.1f;
    }

    public bool IsRunning()
    {
        return isRunning && IsMoving();
    }
}