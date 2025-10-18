using UnityEngine;
using System.Collections;

[System.Serializable]
public class DropItemData
{
    public InventoryItem item;
    [Range(0, 1)]
    public float dropRate = 1f;
}

public class RandomDropEnemy : Attackable
{
    [Header("Drop Settings")]
    [SerializeField] private DropItemData[] possibleDrops;
    [SerializeField] private float knockbackForce = 15f;
    [SerializeField] private float decelerationForce = 8f; // Reducido para frenado más suave
    [SerializeField] private float minSpeedThreshold = 0.2f; // Velocidad mínima antes de detenerse completamente

    private bool isKnockback = false;
    private Vector2 knockbackVelocity;
    private Coroutine knockbackCoroutine;

    protected override void Awake()
    {
        base.Awake();
        healthManager.OnDeath.AddListener(OnDeath);
    }

    public override void TakeDamage(float damage)
    {
        if (!IsAlive()) return;

        base.TakeDamage(damage);

        if (knockbackCoroutine != null)
            StopCoroutine(knockbackCoroutine);

        knockbackCoroutine = StartCoroutine(KnockbackEffect());
    }

    private IEnumerator KnockbackEffect()
    {
        if (isKnockback) yield break;

        isKnockback = true;

        // Calcular dirección y velocidad inicial del knockback
        Vector2 direction = CalculateKnockbackDirection();
        direction.y = 0;
        direction.Normalize();

        knockbackVelocity = direction * knockbackForce;

        // Aplicar velocidad inicial
        rb.linearVelocity = new Vector2(knockbackVelocity.x, rb.linearVelocity.y);

        // Desaceleración progresiva más suave
        while (Mathf.Abs(rb.linearVelocity.x) > minSpeedThreshold)
        {
            // Calcular desaceleración (más suave)
            float deceleration = decelerationForce * Time.fixedDeltaTime;
            float currentSpeed = Mathf.Abs(rb.linearVelocity.x);

            // Reducir velocidad de manera más gradual
            float newSpeed = Mathf.Max(0, currentSpeed - deceleration);
            rb.linearVelocity = new Vector2(
                Mathf.Sign(rb.linearVelocity.x) * newSpeed,
                rb.linearVelocity.y
            );

            yield return new WaitForFixedUpdate();
        }

        // Detener completamente
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        isKnockback = false;
    }

    protected override void OnDeath()
    {
        DropRandomItem();
        Destroy(gameObject);
    }

    private void DropRandomItem()
    {
        if (possibleDrops.Length == 0) return;

        foreach (var drop in possibleDrops)
        {
            if (Random.value <= drop.dropRate)
            {
                if (drop.item != null && drop.item.prefab != null)
                {
                    Instantiate(drop.item.prefab, transform.position, Quaternion.identity);
                }
                break;
            }
        }
    }
}