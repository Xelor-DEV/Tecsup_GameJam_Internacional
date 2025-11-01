using System.Collections.Generic;
using UnityEngine;

public class WaveGroupChecker : MonoBehaviour
{
    [Header("Wave Configuration")]
    [SerializeField] private WavesCombatData wavesData;
    [SerializeField] private string waveName;

    [Header("Teleport Configuration")]
    [SerializeField] private TeleportZone teleportToEnable;

    [Header("Enemies")]
    [SerializeField] private List<Attackable> attackables;

    [Header("References")]
    [SerializeField] private InventoryDatabase database;

    private bool allEnemiesDead = false;
    private bool itemDropped = false;
    private WaveCombat currentWave;

    private void OnEnable()
    {
        database.OnInventoryUpdated += CheckInventoryForRequiredItem;
    }

    private void OnDisable()
    {
        database.OnInventoryUpdated -= CheckInventoryForRequiredItem;
    }

    private void Start()
    {
        // Find the current wave
        if (wavesData != null && !string.IsNullOrEmpty(waveName))
        {
            foreach (WaveCombat wave in wavesData.waves)
            {
                if (wave.waveName == waveName)
                {
                    currentWave = wave;
                    break;
                }
            }
        }

        if (currentWave == null)
        {
            Debug.LogWarning($"Wave '{waveName}' not found in WavesCombatData!");
        }
    }

    private void Update()
    {
        if (!allEnemiesDead)
        {
            CheckAllAttackablesDead();
        }
    }

    private void CheckAllAttackablesDead()
    {
        foreach (Attackable attackable in attackables)
        {
            if (attackable.IsAlive())
                return;
        }

        allEnemiesDead = true;

        CheckInventoryForRequiredItem();
    }

    private void CheckInventoryForRequiredItem()
    {
        if (currentWave == null || database == null || !allEnemiesDead) return;

        // Check if player has the required item in inventory
        List<InventoryItem> requiredItems = database.GetItemsByType(currentWave.requiredItem);

        if (requiredItems.Count > 0)
        {
            // Player has the required item - complete the wave and enable teleport
            currentWave.completed = true;

            if (teleportToEnable != null)
            {
                teleportToEnable.ManualEnable();
            }

            Debug.Log($"Wave '{waveName}' completed! Teleport enabled.");
        }
    }
}
