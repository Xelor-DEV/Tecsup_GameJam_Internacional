using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class UIManager : NonPersistentSingleton<UIManager>
{

    [Header("UI References")]
    [SerializeField] private GameObject selectionsPanel;
    [SerializeField] private GameObject inventorySellPanel;
    [SerializeField] private GameObject inventorySellContent;

    [Header("Inventory System")]
    [SerializeField] private InventoryDatabase dayInventory;
    [SerializeField] private Candy playerCandy;
    [SerializeField] private GameObject inventoryItemUIPrefab;

    [Header("Tween Settings")]
    [SerializeField] private float popInDuration = 0.5f;
    [SerializeField] private float popOutDuration = 0.3f;
    [SerializeField] private Ease popInEase = Ease.OutBack;
    [SerializeField] private Ease popOutEase = Ease.InBack;

    private CharacterUI currentInteractingCharacter;
    private bool isInteracting = false;
    private List<InventoryItemUIDay> spawnedItemUIs = new List<InventoryItemUIDay>();


    void Start()
    {
        DeactivateAllPanels();
    }

    void DeactivateAllPanels()
    {
        if (selectionsPanel != null) selectionsPanel.SetActive(false);
        if (inventorySellPanel != null) inventorySellPanel.SetActive(false);
        if (inventorySellContent != null) inventorySellContent.SetActive(false);
    }

    public bool CanInteractWithCharacter()
    {
        return !isInteracting;
    }

    public void StartCharacterInteraction(CharacterUI character)
    {
        if (isInteracting)
        {
            Debug.Log("Ya hay una interacción en curso");
            return;
        }

        if (character == null)
        {
            Debug.LogError("character es null en StartCharacterInteraction");
            return;
        }

        isInteracting = true;
        currentInteractingCharacter = character;

        // Debug para verificar el personaje actual
        CharacterData charData = character.GetCharacterData();
        Debug.Log($"Iniciando interacción con: {(charData != null ? charData.characterName : "CHARACTER DATA NULL")}");

        StartCoroutine(ShowSelectionsAfterDelay(1f));
    }

    private IEnumerator ShowSelectionsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowSelectionsPanel();
    }

    public void EndCharacterInteraction()
    {
        isInteracting = false;
        currentInteractingCharacter = null;
        HideSelectionsPanel();
        HideInventorySellPanel();
        ClearInventoryItems();
    }

    #region Inventory Management

    private void PopulateInventoryItems()
    {
        if (dayInventory == null)
        {
            Debug.LogError("dayInventory es null en PopulateInventoryItems");
            return;
        }

        if (inventoryItemUIPrefab == null)
        {
            Debug.LogError("inventoryItemUIPrefab es null");
            return;
        }

        if (inventorySellContent == null)
        {
            Debug.LogError("inventorySellContent es null");
            return;
        }

        ClearInventoryItems();

        Debug.Log($"=== POBLANDO INVENTARIO ===");

        // Obtener todos los items del inventario
        for (int i = 0; i < dayInventory.ArraySize; i++)
        {
            InventoryItem item = dayInventory.GetItem(i);
            if (item != null)
            {
                Debug.Log($"Item encontrado en inventario: {item.itemName}, isDropeable: {item.isDroppable}, TargetCharacter: {(item.targetCharacter != null ? item.targetCharacter.characterName : "NULL")}");

                GameObject itemUIObject = Instantiate(inventoryItemUIPrefab, inventorySellContent.transform);
                InventoryItemUIDay itemUI = itemUIObject.GetComponent<InventoryItemUIDay>();
                if (itemUI != null)
                {
                    itemUI.Initialize(item, OnInventoryItemSelected);
                    spawnedItemUIs.Add(itemUI);
                }
                else
                {
                    Debug.LogError($"El prefab {inventoryItemUIPrefab.name} no tiene componente InventoryItemUI");
                }
            }
            else
            {
                Debug.LogWarning($"Slot {i} en inventario está vacío o es null");
            }
        }

        Debug.Log($"Se cargaron {spawnedItemUIs.Count} items en el inventario UI");
        Debug.Log($"=============================");
    }

    private void ClearInventoryItems()
    {
        foreach (InventoryItemUIDay itemUI in spawnedItemUIs)
        {
            if (itemUI != null)
                Destroy(itemUI.gameObject);
        }
        spawnedItemUIs.Clear();
    }

    private void OnInventoryItemSelected(InventoryItem selectedItem)
    {
        if (selectedItem == null)
        {
            Debug.LogError("selectedItem es null en OnInventoryItemSelected");
            return;
        }

        Debug.Log($"Item seleccionado: {selectedItem.itemName}, isDropeable: {selectedItem.isDroppable}");

        if (selectedItem.isDroppable)
        {
            // Vender objeto normal
            SellItem(selectedItem);
        }
        else
        {
            // Intentar dar objeto clave al personaje
            GiveKeyItemToCharacter(selectedItem);
        }
    }
    private void SellItem(InventoryItem item)
    {
        if (item == null)
        {
            Debug.LogError("item es null en SellItem");
            return;
        }

        // Encontrar el UI del item específico
        InventoryItemUIDay itemUI = spawnedItemUIs.Find(ui => ui != null && ui.GetItemData() == item);

        if (itemUI != null)
        {
            itemUI.AnimateSold();
            spawnedItemUIs.Remove(itemUI);
        }
        else
        {
            Debug.LogWarning($"No se encontró UI para vender el item {item.itemName}");
        }

        // Añadir caramelos
        if (playerCandy != null)
        {
            playerCandy.AddCandy(item.candyValue);
            Debug.Log($"Item {item.itemName} vendido por {item.candyValue} caramelos. Total: {playerCandy.amount}");
        }
        else
        {
            Debug.LogError("playerCandy es null");
        }

        // Remover del inventario
        if (dayInventory != null)
        {
            dayInventory.RemoveItem(item);
        }
        else
        {
            Debug.LogError("dayInventory es null");
        }
    }
    private void GiveKeyItemToCharacter(InventoryItem keyItem)
    {
        // Verificaciones exhaustivas de null
        if (currentInteractingCharacter == null)
        {
            Debug.LogError("currentInteractingCharacter es null en GiveKeyItemToCharacter");
            return;
        }

        if (keyItem == null)
        {
            Debug.LogError("keyItem es null en GiveKeyItemToCharacter");
            return;
        }

        // Obtener el CharacterData del personaje actual
        CharacterData characterData = currentInteractingCharacter.GetCharacterData();

        if (characterData == null)
        {
            Debug.LogError("characterData es null en GiveKeyItemToCharacter");
            return;
        }

        // Debug detallado para verificar las asignaciones
        Debug.Log($"=== VERIFICACIÓN DE ASIGNACIONES ===");
        Debug.Log($"Item: {keyItem.itemName}");
        Debug.Log($"Personaje actual: {characterData.characterName}");
        Debug.Log($"TargetCharacter del item: {(keyItem.targetCharacter != null ? keyItem.targetCharacter.characterName : "NULL")}");
        Debug.Log($"¿Coinciden? {keyItem.targetCharacter == characterData}");
        Debug.Log($"=====================================");

        if (keyItem.targetCharacter != null && keyItem.targetCharacter == characterData)
        {
            // Personaje correcto - dar pista
            if (keyItem.relatedClue != null)
            {
                keyItem.relatedClue.MarkAsFound();
                Debug.Log($"¡Pista '{keyItem.relatedClue.clueName}' obtenida!");
            }
            else
            {
                Debug.LogWarning($"El item {keyItem.itemName} no tiene una pista relacionada asignada");
            }

            // Encontrar y eliminar el UI del item correcto
            InventoryItemUIDay itemUI = spawnedItemUIs.Find(ui => ui != null && ui.GetItemData() == keyItem);
            if (itemUI != null)
            {
                itemUI.AnimateSold();
                spawnedItemUIs.Remove(itemUI);
            }
            else
            {
                Debug.LogWarning($"No se encontró el UI para el item {keyItem.itemName}");
            }

            // Remover item del inventario
            if (dayInventory != null)
            {
                dayInventory.RemoveItem(keyItem);
                Debug.Log($"Item {keyItem.itemName} removido del inventario");
            }
            else
            {
                Debug.LogError("dayInventory es null");
            }

            // Cerrar interacción después de éxito
            StartCoroutine(CompleteInteractionAfterDelay(1.5f));
        }
        else
        {
            // Personaje incorrecto - activar leave
            if (keyItem.targetCharacter == null)
            {
                Debug.Log($"El item {keyItem.itemName} no tiene un targetCharacter asignado");
            }
            else
            {
                Debug.Log($"Este personaje ({characterData.characterName}) no necesita este objeto. El objeto es para: {keyItem.targetCharacter.characterName}");
            }
            OnLeaveButtonClicked();
        }
    }
    private IEnumerator CompleteInteractionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        OnLeaveButtonClicked();
    }

    #endregion

    #region Panel Management

    public void ShowSelectionsPanel()
    {
        if (selectionsPanel == null) return;

        selectionsPanel.SetActive(true);
        selectionsPanel.transform.localScale = Vector3.zero;
        selectionsPanel.transform.DOScale(Vector3.one, popInDuration).SetEase(popInEase);
    }

    public void HideSelectionsPanel()
    {
        if (selectionsPanel == null) return;

        selectionsPanel.transform.DOScale(Vector3.zero, popOutDuration)
            .SetEase(popOutEase)
            .OnComplete(() => selectionsPanel.SetActive(false));
    }

    public void ShowInventorySellPanel()
    {
        if (inventorySellPanel == null) return;

        HideSelectionsPanel();
        StartCoroutine(ShowInventoryAfterDelay(0.2f));
    }

    private IEnumerator ShowInventoryAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Poblar inventario antes de mostrar
        PopulateInventoryItems();

        inventorySellPanel.SetActive(true);
        inventorySellPanel.transform.localScale = Vector3.zero;
        inventorySellPanel.transform.DOScale(Vector3.one, popInDuration).SetEase(popInEase);

        if (inventorySellContent != null)
        {
            yield return new WaitForSeconds(0.1f);
            inventorySellContent.SetActive(true);
            inventorySellContent.transform.localScale = Vector3.zero;
            inventorySellContent.transform.DOScale(Vector3.one, popInDuration * 0.8f).SetEase(popInEase);
        }
    }

    public void HideInventorySellPanel()
    {
        if (inventorySellPanel == null) return;

        if (inventorySellContent != null)
        {
            inventorySellContent.transform.DOScale(Vector3.zero, popOutDuration * 0.8f)
                .SetEase(popOutEase)
                .OnComplete(() => inventorySellContent.SetActive(false));
        }

        inventorySellPanel.transform.DOScale(Vector3.zero, popOutDuration)
            .SetEase(popOutEase)
            .OnComplete(() => inventorySellPanel.SetActive(false));
    }

    #endregion

    #region Button Handlers

    public void OnSellButtonClicked()
    {
        ShowInventorySellPanel();
    }

    public void OnLeaveButtonClicked()
    {
        StartCoroutine(LeaveCharacterSequence());
    }

    private IEnumerator LeaveCharacterSequence()
    {
        HideSelectionsPanel();
        HideInventorySellPanel();

        yield return new WaitForSeconds(0.5f);

        if (currentInteractingCharacter != null)
        {
            currentInteractingCharacter.Leave();
        }

        EndCharacterInteraction();
    }

    #endregion

    public void ForceCloseAllUI()
    {
        StopAllCoroutines();
        DeactivateAllPanels();
        ClearInventoryItems();
        isInteracting = false;
        currentInteractingCharacter = null;
    }
}