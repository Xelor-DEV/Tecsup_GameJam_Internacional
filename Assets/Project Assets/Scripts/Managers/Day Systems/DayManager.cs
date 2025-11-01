using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using UnityEngine.SceneManagement;

public class DayManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryDatabase nightInventory;
    [SerializeField] private InventoryDatabase dayInventory;
    [SerializeField] private NightMapConfig nightMapConfig;
    [SerializeField] private CharactersDatabase charactersDatabase;
    [SerializeField] private RectTransform[] spawnPoints;
    [SerializeField] private Canvas targetCanvas;

    [Header("Character Positions")]
    [SerializeField] private RectTransform hidePosition;
    [SerializeField] private RectTransform talkPosition;

    private List<GameObject> spawnedCharacters = new List<GameObject>();

    void Start()
    {
        nightMapConfig.currentDay = nightMapConfig.currentDay + 1;
        InitializeDay();
        SpawnDayCharacters();
    }

    public void InitializeDay()
    {
        ClearSpawnedCharacters();
        TransferNightToDayInventory();
    }

    void TransferNightToDayInventory()
    {
        for (int i = 0; i < nightInventory.ArraySize; i++)
        {
            InventoryItem item = nightInventory.GetItem(i);
            if (item != null)
            {
                dayInventory.AddItem(item);
            }
        }

        for (int i = 0; i < nightInventory.ArraySize; i++)
        {
            nightInventory.RemoveItem(i);
        }
    }

    void SpawnDayCharacters()
    {
        if (charactersDatabase == null || spawnPoints.Length == 0 || targetCanvas == null)
        {
            Debug.LogWarning("Faltan referencias en DayManager");
            return;
        }

        List<CharacterData> availableCharacters = charactersDatabase.characters
            .Where(c => c.availableDays.Contains(nightMapConfig.currentDay))
            .ToList();

        if (availableCharacters.Count == 0)
        {
            Debug.Log($"No hay personajes disponibles para el día {nightMapConfig.currentDay}");
            return;
        }

        // Mezclar puntos de spawn
        System.Random rng = new System.Random();
        List<RectTransform> shuffledSpawns = spawnPoints.OrderBy(x => rng.Next()).ToList();

        int spawnIndex = 0;
        foreach (CharacterData character in availableCharacters)
        {
            if (spawnIndex >= shuffledSpawns.Count)
            {
                Debug.LogWarning("No hay suficientes puntos de spawn para todos los personajes");
                break;
            }

            RectTransform spawnPoint = shuffledSpawns[spawnIndex];
            GameObject newCharacter = Instantiate(
                character.characterPrefab,
                spawnPoint // Spawn como hijo del spawn point
            );

            // Configurar el RectTransform del personaje
            RectTransform charRect = newCharacter.GetComponent<RectTransform>();
            if (charRect != null)
            {
                // Establecer todos los valores a 0
                charRect.anchorMin = Vector2.zero;
                charRect.anchorMax = Vector2.one;
                charRect.offsetMin = Vector2.zero; // Left y Bottom
                charRect.offsetMax = Vector2.zero; // Right y Top
                charRect.pivot = new Vector2(0.5f, 0.5f); // Centro por defecto
                charRect.localScale = Vector3.one;
                charRect.localPosition = Vector3.zero;
            }

            // Inicializar el CharacterUI
            CharacterUI characterUI = newCharacter.GetComponent<CharacterUI>();
            if (characterUI != null)
            {
                characterUI.Initialize(this);
            }

            spawnedCharacters.Add(newCharacter);
            spawnIndex++;

            Debug.Log($"Personaje {character.characterName} spawnado como hijo del punto {spawnIndex}");
        }
    }

    void ClearSpawnedCharacters()
    {
        foreach (GameObject character in spawnedCharacters)
        {
            if (character != null)
                Destroy(character);
        }
        spawnedCharacters.Clear();
    }

    // Métodos públicos para que CharacterUI acceda a las posiciones
    public RectTransform GetHidePosition()
    {
        return hidePosition;
    }

    public RectTransform GetTalkPosition()
    {
        return talkPosition;
    }

    public void Scene(string scene)
    {
        SceneManager.LoadScene(scene);
    }
}