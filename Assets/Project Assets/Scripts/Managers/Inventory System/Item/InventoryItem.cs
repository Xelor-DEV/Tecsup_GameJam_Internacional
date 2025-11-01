using UnityEngine;

[CreateAssetMenu(fileName = "InventoryItem", menuName = "Game/Inventory Item")]
public class InventoryItem : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public bool isDroppable = true;
    public int candyValue = 5; // Valor por defecto para objetos vendibles
    public Clue relatedClue; // Referencia a la pista si es objeto clave
    public CharacterData targetCharacter; // Personaje que necesita este objeto}
    public GameObject prefab;
    public ItemType itemType;
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