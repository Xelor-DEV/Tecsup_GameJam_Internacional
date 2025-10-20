using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class InventoryItemUIDay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private Button itemButton;
    [SerializeField] private GameObject keyItemIndicator;

    private InventoryItem itemData;
    private System.Action<InventoryItem> onItemSelected;

    public void Initialize(InventoryItem item, System.Action<InventoryItem> onSelect)
    {
        itemData = item;
        onItemSelected = onSelect;

        if (itemIcon != null)
            itemIcon.sprite = item.icon;

        if (keyItemIndicator != null)
            keyItemIndicator.SetActive(!item.isDroppable);

        if (itemButton != null)
            itemButton.onClick.AddListener(OnItemClicked);

        // Animación de aparición
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
    }

    // Nuevo método para obtener los datos del item
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