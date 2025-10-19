using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DayManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryDatabase nightInventory;
    [SerializeField] private InventoryDatabase dayInventory;

    [SerializeField] private RectTransform test;
    [SerializeField] private RectTransform test2;

    private void Start()
    {
        test.position = test2.position;
    }

    public void InitializeDay()
    {
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

}