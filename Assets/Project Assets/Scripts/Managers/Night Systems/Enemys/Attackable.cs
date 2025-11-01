using UnityEngine;

public abstract class Attackable : MonoBehaviour
{
    [SerializeField] protected HealthManager healthManager;
    protected PlayerMovement playerMovement;
    [SerializeField] protected Rigidbody2D rb;

    public PlayerMovement PlayerMovement
    {
        set
        {
            playerMovement = value;
        }
    }

    protected virtual void Awake()
    {
        if (healthManager == null)
            healthManager = GetComponent<HealthManager>();
    }

    public virtual void TakeDamage(float damage)
    {
        healthManager?.TakeDamage(damage);
    }

    public virtual bool IsAlive()
    {
        return healthManager != null && healthManager.CurrentHealth > 0;
    }

    protected abstract void OnDeath();

    protected Vector2 CalculateKnockbackDirection()
    {
        Vector2 playerToBox = (Vector2)(transform.position - playerMovement.transform.position);

        if (playerMovement != null)
        {
            Vector2 movementDirection = playerMovement.GetMovementDirection();
            if (movementDirection.magnitude > 0.1f)
            {
                return movementDirection;
            }
        }

        return playerToBox.normalized;
    }
}