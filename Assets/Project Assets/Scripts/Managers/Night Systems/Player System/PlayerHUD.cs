using UnityEngine;
using System.Collections.Generic;

public class PlayerHUD : MonoBehaviour
{
    [Header("Inventory References")]
    [SerializeField] private InventoryDatabase inventoryDatabase;
    [SerializeField] private Transform inventoryContainer;
    [SerializeField] private GameObject inventoryItemPrefab;

    private Dictionary<int, GameObject> spawnedItems = new Dictionary<int, GameObject>();

    private void OnEnable()
    {
        if (inventoryDatabase != null)
        {
            inventoryDatabase.OnItemAdded += HandleItemAdded;
            inventoryDatabase.OnItemRemoved += HandleItemRemoved;
            inventoryDatabase.OnInventoryUpdated += HandleFullUpdate;
        }
    }

    private void OnDisable()
    {
        if (inventoryDatabase != null)
        {
            inventoryDatabase.OnItemAdded -= HandleItemAdded;
            inventoryDatabase.OnItemRemoved -= HandleItemRemoved;
            inventoryDatabase.OnInventoryUpdated -= HandleFullUpdate;
        }
    }

    private void Start()
    {
        HandleFullUpdate();
    }

    private void HandleItemAdded(InventoryItem item, int index)
    {
        GameObject newItemUI = Instantiate(inventoryItemPrefab, inventoryContainer);
        InventoryItemUI itemUI = newItemUI.GetComponent<InventoryItemUI>();

        if (itemUI != null)
        {
            itemUI.Setup(item);
        }

        spawnedItems[index] = newItemUI;
    }

    private void HandleItemRemoved(int index)
    {
        if (spawnedItems.ContainsKey(index))
        {
            Destroy(spawnedItems[index]);
            spawnedItems.Remove(index);
        }
    }

    private void HandleFullUpdate()
    {
        foreach (Transform child in inventoryContainer)
        {
            Destroy(child.gameObject);
        }
        spawnedItems.Clear();

        for (int i = 0; i < inventoryDatabase.ArraySize; i++)
        {
            InventoryItem item = inventoryDatabase.GetItem(i);
            if (item != null)
            {
                HandleItemAdded(item, i);
            }
        }
    }
}