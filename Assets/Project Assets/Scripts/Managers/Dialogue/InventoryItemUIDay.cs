using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class InventoryItemUIDay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private Button itemButton;

    private InventoryItem itemData;
    private System.Action<InventoryItem> onItemSelected;

    public void Initialize(InventoryItem item, System.Action<InventoryItem> onSelect)
    {
        itemData = item;
        onItemSelected = onSelect;

        if (itemIcon != null)
            itemIcon.sprite = item.icon;

        if (itemButton != null)
        {
            // Si no hay callback, deshabilitar el botón (modo vista)
            if (onSelect == null)
            {
                itemButton.interactable = false;
            }
            else
            {
                itemButton.onClick.AddListener(OnItemClicked);
            }
        }

        // Animación de aparición
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
    }

    public InventoryItem GetItemData()
    {
        return itemData;
    }

    private void OnItemClicked()
    {
        onItemSelected?.Invoke(itemData);
    }

    public void AnimateSold()
    {
        transform.DOScale(Vector3.zero, 0.3f)
            .SetEase(Ease.InBack)
            .OnComplete(() => Destroy(gameObject));
    }

    void OnDestroy()
    {
        if (itemButton != null)
            itemButton.onClick.RemoveAllListeners();
    }
}