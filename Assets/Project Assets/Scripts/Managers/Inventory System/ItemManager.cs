using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryItem item;

    public InventoryItem GetItem()
    {
        return item;
    }

    public void SetItem(InventoryItem newItem)
    {
        item = newItem;
    }
}