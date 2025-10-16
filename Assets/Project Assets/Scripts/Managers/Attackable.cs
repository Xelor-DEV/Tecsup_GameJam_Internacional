using UnityEngine;

public class Attackable : MonoBehaviour
{
    [SerializeField] private float health = 30f;
    [SerializeField] private bool isAlive = true;

    public void TakeDamage(float damage)
    {
        if (!isAlive) return;

        health -= damage;
        Debug.Log($"{gameObject.name} recibió {damage} de daño. Vida restante: {health}");

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isAlive = false;
        Debug.Log($"{gameObject.name} ha sido derrotado!");

        // Aquí puedes agregar animación de muerte, sonidos, etc.
        Destroy(gameObject, 0.1f); // Destruir después de un pequeño delay
    }

    public bool IsAlive()
    {
        return isAlive;
    }
}