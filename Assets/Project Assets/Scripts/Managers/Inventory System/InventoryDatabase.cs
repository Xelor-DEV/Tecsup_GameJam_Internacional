using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "Inventory Database", menuName = "Inventory/Database")]
public class InventoryDatabase : ScriptableObject
{
    [Header("Configuration")]
    [SerializeField] private int maxSize = 10;

    [Header("Items")]
    [SerializeField] private InventoryItem[] items;

    public event Action<InventoryItem, int> OnItemAdded;
    public event Action<int> OnItemRemoved;
    public event Action OnInventoryUpdated;

    public int ArraySize => items.Length;
    public int MaxSize => maxSize;

    private void OnValidate()
    {
        if (items == null)
        {
            items = new InventoryItem[Mathf.Max(0, maxSize)];
        }
        else if (items.Length != maxSize)
        {
            System.Array.Resize(ref items, Mathf.Max(0, maxSize));
        }
    }

    public InventoryItem GetItem(int index)
    {
        if (index >= 0 && index < items.Length)
            return items[index];
        return null;
    }

    public List<InventoryItem> GetItemsByType(ItemType type)
    {
        List<InventoryItem> foundItems = new List<InventoryItem>();
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null && items[i].itemType == type)
                foundItems.Add(items[i]);
        }
        return foundItems;
    }

    public bool AddItem(InventoryItem newItem)
    {
        for (int i = 0; i < Mathf.Min(items.Length, maxSize); i++)
        {
            if (items[i] == null)
            {
                items[i] = newItem;
                OnItemAdded?.Invoke(newItem, i);
                OnInventoryUpdated?.Invoke();
                return true;
            }
        }

        if (items.Length < maxSize)
        {
            int newSize = Mathf.Min(items.Length + 1, maxSize);
            InventoryItem[] newArray = new InventoryItem[newSize];

            for (int i = 0; i < items.Length; i++)
            {
                newArray[i] = items[i];
            }
            newArray[items.Length] = newItem;
            items = newArray;
            OnItemAdded?.Invoke(newItem, items.Length - 1);
            OnInventoryUpdated?.Invoke();
            return true;
        }

        Debug.LogWarning("No se puede agregar el ítem: Inventario lleno");
        return false;
    }

    public void RemoveItem(int index)
    {
        if (index >= 0 && index < items.Length)
        {
            items[index] = null;
            OnItemRemoved?.Invoke(index);
            OnInventoryUpdated?.Invoke();
        }
    }

    public void ResizeArray(int newSize)
    {
        newSize = Mathf.Clamp(newSize, 0, maxSize);
        InventoryItem[] newArray = new InventoryItem[newSize];

        int minLength = Mathf.Min(newSize, items.Length);
        for (int i = 0; i < minLength; i++)
        {
            newArray[i] = items[i];
        }

        items = newArray;
        OnInventoryUpdated?.Invoke();
    }
}