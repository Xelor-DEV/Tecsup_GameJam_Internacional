using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class InventoryItem : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public ItemType itemType;
    public GameObject prefab;
}

public enum ItemType
{
    Sandals,
    Monster,
    Book,
    Flowers,
    Bolts,
    CannedTuna,
    SewingKit,
    EyeDrops
}