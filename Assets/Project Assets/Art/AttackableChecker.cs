using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AttackableChecker : MonoBehaviour
{
    [SerializeField] private Attackable[] attackables;
    [SerializeField] private string sceneName = "Day";
    [SerializeField] private float checkInterval = 0.5f;
    [SerializeField] private InventoryDatabase inventoryDatabase; // Referencia al inventario
    [SerializeField] private NightMapConfig nightMapConfig;

    private float timer;
    private bool allEnemiesDead = false;
    private bool hasMinimumItem = false;

    private void Start()
    {
        // Desactivar el script si no es el día 0
        if (!IsDayZero())
        {
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        // Verificar si es día 0 (por si acaso)
        if (!IsDayZero())
        {
            enabled = false;
            return;
        }

        timer += Time.deltaTime;

        if (timer >= checkInterval)
        {
            timer = 0;

            if (!allEnemiesDead)
            {
                CheckAllAttackables();
            }
            else if (!hasMinimumItem)
            {
                CheckInventory();
            }
            else
            {
                // Ambos condiciones cumplidas, iniciar transición
                StartCoroutine(TransitionToDayScene());
                enabled = false; // Desactivar después de iniciar la transición
            }
        }
    }

    private bool IsDayZero()
    {
        // Verificar si estamos en el día 0
        return nightMapConfig != null && nightMapConfig.currentDay == 0;
    }

    private void CheckAllAttackables()
    {
        if (attackables == null || attackables.Length == 0)
        {
            allEnemiesDead = true;
            return;
        }

        foreach (Attackable attackable in attackables)
        {
            if (attackable != null && attackable.IsAlive())
            {
                return; // Si encuentra al menos uno vivo, sale de la función
            }
        }

        // Si llegamos aquí, todos están muertos
        allEnemiesDead = true;
        Debug.Log("Todos los enemigos están muertos. Verificando inventario...");
    }

    private void CheckInventory()
    {
        if (inventoryDatabase == null)
        {
            Debug.LogWarning("InventoryDatabase no asignado");
            return;
        }

        // Verificar si hay al menos un item en el inventario
        for (int i = 0; i < inventoryDatabase.ArraySize; i++)
        {
            if (inventoryDatabase.GetItem(i) != null)
            {
                hasMinimumItem = true;
                Debug.Log("Inventario verificado: se encontró al menos un item");
                return;
            }
        }

        // Si no hay items, continuará verificando en el próximo intervalo
        Debug.Log("Esperando que el jugador recoja al menos un item...");
    }

    private IEnumerator TransitionToDayScene()
    {
        Debug.Log("Transición a escena del día iniciada");

        // Fade de transparente a negro
        if (CameraManager.Instance != null)
        {
            yield return CameraManager.Instance.StartCoroutine(CameraManager.Instance.Fade(0f, 1f));
        }
        else
        {
            // Fallback si no hay CameraManager
            yield return new WaitForSecondsRealtime(1f);
        }

        // Esperar un momento en negro
        yield return new WaitForSecondsRealtime(0.5f);

        // Cargar la escena del día
        SceneManager.LoadScene(sceneName);
    }

    // Opcional: Método para actualizar la lista en tiempo de ejecución
    public void UpdateAttackablesList(Attackable[] newAttackables)
    {
        attackables = newAttackables;
        allEnemiesDead = false; // Resetear el estado al actualizar la lista
    }

    // Método para forzar la verificación manualmente
    public void ForceCheck()
    {
        timer = checkInterval; // Forzar verificación en el próximo Update
    }

    // Métodos para debug
    public bool AreAllEnemiesDead() => allEnemiesDead;
    public bool HasMinimumItem() => hasMinimumItem;
}