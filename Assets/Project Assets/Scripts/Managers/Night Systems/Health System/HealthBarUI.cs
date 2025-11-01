using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HealthManager healthManager;
    [SerializeField] private Image healthBarImage;

    private void Start()
    {
        if (healthManager != null)
        {
            healthManager.OnHealthChanged.AddListener(UpdateHealthBar);
            UpdateHealthBar(healthManager.CurrentHealth, healthManager.MaxHealth);
        }
    }

    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthBarImage != null)
        {
            healthBarImage.fillAmount = currentHealth / maxHealth;
        }
    }
}