using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItemUI : MonoBehaviour
{
    [SerializeField] private Image icon;

    public void Setup(InventoryItem item)
    {
        if (icon != null) icon.sprite = item.icon;
    }
}