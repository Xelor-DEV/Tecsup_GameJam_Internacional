using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] ParticleSystem runParticles;

    [Header("X Axis Movement Settings")]
    [SerializeField] private float walkSpeedX = 6f;
    [SerializeField] private float runSpeedX = 10f;
    [SerializeField] private float accelerationX = 25f;
    [SerializeField] private float decelerationX = 30f;

    [Header("Y Axis Movement Settings")]
    [SerializeField] private float walkSpeedY = 4f;
    [SerializeField] private float runSpeedY = 6f;
    [SerializeField] private float accelerationY = 20f;
    [SerializeField] private float decelerationY = 25f;
    [SerializeField] private float velocityPower = 0.9f;

    [Header("Direction Change Settings")]
    [SerializeField] private float directionChangeMultiplier = 2f;
    [SerializeField] private float directionChangeThreshold = 0.3f;

    [Header("Smooth Stop Settings")]
    [SerializeField] private float stopSmoothTime = 0.08f;

    [Header("Animation Parameters")]
    [SerializeField] private string isMovingBool = "IsMoving";
    [SerializeField] private string isRunningBool = "IsRunning";

    [Header("Particle Settings")]
    [SerializeField] private float particleEmissionRate = 20f;

    private Vector2 moveInput;
    private bool isRunning = false;
    private Vector2 currentSpeed;
    private bool canMove = true;

    private bool wasMoving = false;
    private bool wasRunning = false;
    private Vector2 smoothStopVelocity;
    private Vector2 lastMoveInput;

    private ParticleSystem.EmissionModule emissionModule;
    private bool particlesShouldBeEmitting = false;

    private void Awake()
    {
        currentSpeed = new Vector2(walkSpeedX, walkSpeedY);
        lastMoveInput = Vector2.zero;

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearDamping = 0f;
            rb.angularDamping = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        if (runParticles != null)
        {
            emissionModule = runParticles.emission;
            // Inicialmente deshabilitamos la emisión
            emissionModule.enabled = false;

            // Aseguramos que el sistema esté listo para usar
            if (!runParticles.isPlaying)
            {
                runParticles.Play();
            }
        }
    }

    private void Update()
    {
        UpdateAnimations();
        HandleSpriteFlip();
        UpdateParticleSystem();
    }

    private void FixedUpdate()
    {
        if (canMove)
        {
            HandleMovement();
        }
        else
        {
            ApplyDeceleration();
        }

        lastMoveInput = moveInput;
    }

    public void GetMovementInput(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        moveInput = input.normalized;
    }

    public void GetRunInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isRunning = true;
            currentSpeed = new Vector2(runSpeedX, runSpeedY);
        }
        else if (context.canceled)
        {
            isRunning = false;
            currentSpeed = new Vector2(walkSpeedX, walkSpeedY);
        }
    }

    private void HandleMovement()
    {
        Vector2 targetVelocity = new Vector2(
            moveInput.x * currentSpeed.x,
            moveInput.y * currentSpeed.y
        );

        if (moveInput.magnitude < 0.1f)
        {
            rb.linearVelocity = Vector2.SmoothDamp(rb.linearVelocity, Vector2.zero, ref smoothStopVelocity, stopSmoothTime);
            return;
        }

        bool isChangingDirectionX = Mathf.Sign(moveInput.x) != Mathf.Sign(lastMoveInput.x) && Mathf.Abs(moveInput.x) > directionChangeThreshold && Mathf.Abs(lastMoveInput.x) > directionChangeThreshold;
        bool isChangingDirectionY = Mathf.Sign(moveInput.y) != Mathf.Sign(lastMoveInput.y) && Mathf.Abs(moveInput.y) > directionChangeThreshold && Mathf.Abs(lastMoveInput.y) > directionChangeThreshold;

        Vector2 velocityDifference = targetVelocity - rb.linearVelocity;

        float movementForceX = CalculateMovementForce(velocityDifference.x, isChangingDirectionX, true);
        float movementForceY = CalculateMovementForce(velocityDifference.y, isChangingDirectionY, false);

        rb.AddForce(new Vector2(movementForceX, movementForceY), ForceMode2D.Force);

        ClampVelocity();
    }

    private float CalculateMovementForce(float velocityDiff, bool isChangingDirection, bool isXAxis)
    {
        float absVelocityDiff = Mathf.Abs(velocityDiff);

        if (absVelocityDiff < 0.01f) return 0f;

        float accelerationRate = isXAxis ? accelerationX : accelerationY;
        float decelerationRate = isXAxis ? decelerationX : decelerationY;

        if (isChangingDirection)
        {
            decelerationRate *= directionChangeMultiplier;
            return Mathf.Sign(velocityDiff) * Mathf.Pow(absVelocityDiff, velocityPower) * decelerationRate;
        }

        bool shouldAccelerate = Mathf.Abs((isXAxis ? moveInput.x : moveInput.y)) > 0.1f;
        float currentAccelerationRate = shouldAccelerate ? accelerationRate : decelerationRate;

        return Mathf.Sign(velocityDiff) * Mathf.Pow(absVelocityDiff, velocityPower) * currentAccelerationRate;
    }

    private void ClampVelocity()
    {
        Vector2 currentVel = rb.linearVelocity;

        if (Mathf.Abs(currentVel.x) > currentSpeed.x)
        {
            currentVel.x = Mathf.Sign(currentVel.x) * currentSpeed.x;
        }

        if (Mathf.Abs(currentVel.y) > currentSpeed.y)
        {
            currentVel.y = Mathf.Sign(currentVel.y) * currentSpeed.y;
        }

        rb.linearVelocity = currentVel;
    }

    private void ApplyDeceleration()
    {
        Vector2 currentVel = rb.linearVelocity;

        if (Mathf.Abs(currentVel.x) > 0.1f)
        {
            float decelerationForceX = Mathf.Sign(currentVel.x) * decelerationX * 2f * -1f;
            rb.AddForce(new Vector2(decelerationForceX, 0), ForceMode2D.Force);
        }
        else
        {
            currentVel.x = 0f;
        }

        if (Mathf.Abs(currentVel.y) > 0.1f)
        {
            float decelerationForceY = Mathf.Sign(currentVel.y) * decelerationY * 2f * -1f;
            rb.AddForce(new Vector2(0, decelerationForceY), ForceMode2D.Force);
        }
        else
        {
            currentVel.y = 0f;
        }

        rb.linearVelocity = currentVel;
    }

    private void HandleSpriteFlip()
    {
        if (moveInput.x > 0.2f)
        {
            spriteRenderer.flipX = false;
        }
        else if (moveInput.x < -0.2f)
        {
            spriteRenderer.flipX = true;
        }
    }

    private void UpdateAnimations()
    {
        if (animator != null)
        {
            bool isMoving = IsMoving();
            bool isActuallyRunning = IsRunning();

            if (isMoving != wasMoving)
            {
                animator.SetBool(isMovingBool, isMoving);
                wasMoving = isMoving;
            }

            if (isActuallyRunning != wasRunning)
            {
                animator.SetBool(isRunningBool, isActuallyRunning);
                wasRunning = isActuallyRunning;
            }
        }
    }

    private void UpdateParticleSystem()
    {
        if (runParticles != null)
        {
            bool newEmissionState = IsRunning();

            // Solo cambiar el estado si es diferente al anterior
            if (newEmissionState != particlesShouldBeEmitting)
            {
                particlesShouldBeEmitting = newEmissionState;
                emissionModule.enabled = particlesShouldBeEmitting;

                if (particlesShouldBeEmitting)
                {
                    // Asegurarse de que el sistema esté reproduciéndose
                    if (!runParticles.isPlaying)
                    {
                        runParticles.Play();
                    }

                    // Ajustar la tasa de emisión basada en la velocidad
                    float speedFactor = Mathf.Clamp01(rb.linearVelocity.magnitude / currentSpeed.magnitude);
                    emissionModule.rateOverTime = particleEmissionRate * speedFactor;
                }
                else
                {
                    // Cuando se detiene, solo deshabilitamos la emisión
                    // Las partículas existentes continuarán su ciclo natural
                    emissionModule.enabled = false;
                }
            }

            // Si estamos emitiendo, actualizar la tasa basada en la velocidad
            if (particlesShouldBeEmitting)
            {
                float speedFactor = Mathf.Clamp01(rb.linearVelocity.magnitude / currentSpeed.magnitude);
                emissionModule.rateOverTime = particleEmissionRate * speedFactor;
            }
        }
    }

    public void SetMovementEnabled(bool enabled)
    {
        canMove = enabled;
        if (!enabled)
        {
            moveInput = Vector2.zero;
            smoothStopVelocity = Vector2.zero;

            if (runParticles != null)
            {
                particlesShouldBeEmitting = false;
                emissionModule.enabled = false;
            }
        }
    }

    public Vector2 GetMovementDirection()
    {
        return moveInput;
    }

    public bool IsMoving()
    {
        return moveInput.magnitude > 0.1f && canMove;
    }

    public bool IsRunning()
    {
        return isRunning && IsMoving() && rb.linearVelocity.magnitude > 0.5f;
    }

    public bool CanMove()
    {
        return canMove;
    }
}