using UnityEngine;
using System.Collections;

public class PlayerKnockback : MonoBehaviour
{
    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForce = 15f;
    [SerializeField] private float decelerationForce = 8f;
    [SerializeField] private float minSpeedThreshold = 0.2f;
    [SerializeField] private float stunDuration = 0.2f;

    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private PlayerCombat playerCombat;

    private bool isKnockback = false;
    private Coroutine knockbackCoroutine;
    private Vector2 knockbackVelocity;


    private void Awake()
    {
        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
        if (playerCombat == null)
            playerCombat = GetComponent<PlayerCombat>();
    }

    public void ApplyKnockback()
    {
        if (isKnockback && knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
        }

        knockbackCoroutine = StartCoroutine(KnockbackEffect());
    }

    private IEnumerator KnockbackEffect()
    {
        isKnockback = true;

        // Interrumpir ataque del jugador si está atacando
        if (playerCombat != null && playerCombat.IsAttacking)
        {
            playerCombat.InterruptAttack();
        }

        Vector2 direction = CalculateKnockbackDirection();
        direction.y = 0;
        direction.Normalize();

        knockbackVelocity = direction * knockbackForce;
        rb.linearVelocity = new Vector2(knockbackVelocity.x, rb.linearVelocity.y);

        while (Mathf.Abs(rb.linearVelocity.x) > minSpeedThreshold)
        {
            float deceleration = decelerationForce * Time.fixedDeltaTime;
            float currentSpeed = Mathf.Abs(rb.linearVelocity.x);
            float newSpeed = Mathf.Max(0, currentSpeed - deceleration);
            rb.linearVelocity = new Vector2(
                Mathf.Sign(rb.linearVelocity.x) * newSpeed,
                rb.linearVelocity.y
            );
            yield return new WaitForFixedUpdate();
        }

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        yield return new WaitForSeconds(stunDuration);

        isKnockback = false;
    }

    private Vector2 CalculateKnockbackDirection()
    {
        // Buscar el enemigo más cercano
        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, 5f, LayerMask.GetMask("Characters"));

        Vector2 direction = Vector2.zero;
        float closestDistance = Mathf.Infinity;

        foreach (Collider2D enemy in nearbyEnemies)
        {
            if (enemy.GetComponent<EntityIdentifier>().Entity == Entity.Enemy) 
            {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    direction = (transform.position - enemy.transform.position).normalized;
                }
            }

        }

        // Si no se encontraron enemigos, usar dirección aleatoria con componente vertical
        if (direction == Vector2.zero)
        {
            direction = new Vector2(Random.Range(-1f, 1f), 0.5f).normalized;
        }

        return direction;
    }

    public bool IsInKnockback()
    {
        return isKnockback;
    }
}