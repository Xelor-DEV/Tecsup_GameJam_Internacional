using UnityEngine;
using System.Collections;

public class GuaranteedDropEnemy : Attackable
{
    [Header("Guaranteed Drop Settings")]
    [SerializeField] private InventoryItem guaranteedDrop;
    [SerializeField] private float knockbackForce = 15f;
    [SerializeField] private float decelerationForce = 8f;
    [SerializeField] private float minSpeedThreshold = 0.2f;
    [SerializeField] private float stunDuration = 0.2f;
    [SerializeField] private EnemyCombat enemyCombat;

    private EnemyAI enemyAI;

    private bool isKnockback = false;
    private Vector2 knockbackVelocity;

    protected override void Awake()
    {
        base.Awake();
        enemyAI = GetComponent<EnemyAI>();
        healthManager.OnDeath.AddListener(OnDeath);
    }

    public override void TakeDamage(float damage)
    {
        if (!IsAlive()) return;

        base.TakeDamage(damage);

        // NUEVO: Interrumpir ataque si está atacando
        if (enemyCombat != null && enemyCombat.IsAttacking)
        {
            enemyCombat.InterruptAttack();
        }

        if (!isKnockback)
        {
            StartCoroutine(KnockbackEffect());
        }
    }

    // Resto del código permanece igual...
    private IEnumerator KnockbackEffect()
    {
        isKnockback = true;
        enemyAI.SetStunned(true);

        Vector2 direction = CalculateKnockbackDirection();
        direction.y = 0;
        direction.Normalize();

        knockbackVelocity = direction * knockbackForce;
        enemyAI.Rb.linearVelocity = new Vector2(knockbackVelocity.x, enemyAI.Rb.linearVelocity.y);

        while (Mathf.Abs(enemyAI.Rb.linearVelocity.x) > minSpeedThreshold)
        {
            float deceleration = decelerationForce * Time.fixedDeltaTime;
            float currentSpeed = Mathf.Abs(enemyAI.Rb.linearVelocity.x);
            float newSpeed = Mathf.Max(0, currentSpeed - deceleration);
            enemyAI.Rb.linearVelocity = new Vector2(
                Mathf.Sign(enemyAI.Rb.linearVelocity.x) * newSpeed,
                enemyAI.Rb.linearVelocity.y
            );
            yield return new WaitForFixedUpdate();
        }

        enemyAI.Rb.linearVelocity = new Vector2(0, enemyAI.Rb.linearVelocity.y);

        yield return new WaitForSeconds(stunDuration);

        isKnockback = false;
        enemyAI.SetStunned(false);
    }

    protected override void OnDeath()
    {
        DropGuaranteedItem();
        Destroy(gameObject);
    }

    private void DropGuaranteedItem()
    {
        if (guaranteedDrop != null && guaranteedDrop.prefab != null)
        {
            Instantiate(guaranteedDrop.prefab, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Guaranteed drop item is not assigned or missing prefab!", this);
        }
    }
}