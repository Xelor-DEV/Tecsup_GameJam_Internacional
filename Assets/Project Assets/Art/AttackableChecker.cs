using UnityEngine;
using UnityEngine.SceneManagement;

public class AttackableChecker : MonoBehaviour
{
    [SerializeField] private Attackable[] attackables;
    [SerializeField] private string sceneName = "Day";
    [SerializeField] private float checkInterval = 0.5f; // Para optimizar no verificar cada frame
    private float timer;

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= checkInterval)
        {
            timer = 0;
            CheckAllAttackables();
        }
    }

    private void CheckAllAttackables()
    {
        if (attackables == null || attackables.Length == 0) return;

        foreach (Attackable attackable in attackables)
        {
            if (attackable != null && attackable.IsAlive())
            {
                return; // Si encuentra al menos uno vivo, sale de la función
            }
        }

        // Si llegamos aquí, todos están muertos
        LoadScene();
    }

    private void LoadScene()
    {
        SceneManager.LoadScene(sceneName);
    }

    // Opcional: Método para actualizar la lista en tiempo de ejecución
    public void UpdateAttackablesList(Attackable[] newAttackables)
    {
        attackables = newAttackables;
    }
}