using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIManager : NonPersistentSingleton<UIManager>
{

    [Header("UI References")]
    [SerializeField] private GameObject selectionsPanel;
    [SerializeField] private GameObject inventorySellPanel;
    [SerializeField] private GameObject inventorySellContent;
    [SerializeField] private GameObject inventoryViewPanel;
    [SerializeField] private GameObject inventoryViewContent;
    [SerializeField] private Button playerInventoryButton;
    [SerializeField] private Button goToNightButton;
    [SerializeField] private Button backButtonSell;
    [SerializeField] private Button backButtonView;

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
    private List<InventoryItemUIDay> spawnedViewItemUIs = new List<InventoryItemUIDay>();

    void Start()
    {
        DeactivateAllPanels();
        SetupButtonListeners();
        ShowMainButtons(); // Mostrar botones principales al inicio
    }

    void SetupButtonListeners()
    {
        // Configurar listeners de los nuevos botones
        if (playerInventoryButton != null)
            playerInventoryButton.onClick.AddListener(OnPlayerInventoryClicked);

        if (goToNightButton != null)
            goToNightButton.onClick.AddListener(OnGoToNightClicked);

        // Configurar botones de back
        if (backButtonSell != null)
            backButtonSell.onClick.AddListener(OnBackButtonClicked);

        if (backButtonView != null)
            backButtonView.onClick.AddListener(OnBackButtonClicked);
    }

    void DeactivateAllPanels()
    {
        if (selectionsPanel != null) selectionsPanel.SetActive(false);
        if (inventorySellPanel != null) inventorySellPanel.SetActive(false);
        if (inventorySellContent != null) inventorySellContent.SetActive(false);
        if (inventoryViewPanel != null) inventoryViewPanel.SetActive(false);
        if (inventoryViewContent != null) inventoryViewContent.SetActive(false);
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

        HideMainButtons();

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
        HideInventoryViewPanel();
        ClearInventoryItems();

        // Mostrar botones principales al finalizar interacción
        ShowMainButtons();
    }
    #region Main Buttons Management

    private void ShowMainButtons()
    {
        if (playerInventoryButton != null)
        {
            playerInventoryButton.gameObject.SetActive(true);
            playerInventoryButton.transform.localScale = Vector3.zero;
            playerInventoryButton.transform.DOScale(Vector3.one, popInDuration).SetEase(popInEase);
        }

        if (goToNightButton != null)
        {
            goToNightButton.gameObject.SetActive(true);
            goToNightButton.transform.localScale = Vector3.zero;
            goToNightButton.transform.DOScale(Vector3.one, popInDuration).SetEase(popInEase);
        }
    }

    private void HideMainButtons()
    {
        if (playerInventoryButton != null)
        {
            playerInventoryButton.transform.DOScale(Vector3.zero, popOutDuration)
                .SetEase(popOutEase)
                .OnComplete(() => playerInventoryButton.gameObject.SetActive(false));
        }

        if (goToNightButton != null)
        {
            goToNightButton.transform.DOScale(Vector3.zero, popOutDuration)
                .SetEase(popOutEase)
                .OnComplete(() => goToNightButton.gameObject.SetActive(false));
        }
    }

    #endregion

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

    private void PopulateInventoryView()
    {
        if (dayInventory == null || inventoryItemUIPrefab == null || inventoryViewContent == null)
        {
            Debug.LogError("Referencias faltantes en PopulateInventoryView");
            return;
        }

        ClearInventoryViewItems();

        Debug.Log("=== POBLANDO VISTA DE INVENTARIO ===");

        for (int i = 0; i < dayInventory.ArraySize; i++)
        {
            InventoryItem item = dayInventory.GetItem(i);
            if (item != null)
            {
                GameObject itemUIObject = Instantiate(inventoryItemUIPrefab, inventoryViewContent.transform);
                InventoryItemUIDay itemUI = itemUIObject.GetComponent<InventoryItemUIDay>();
                if (itemUI != null)
                {
                    // En el modo vista, no pasamos callback o pasamos null para deshabilitar interacciones
                    itemUI.Initialize(item, null);
                    spawnedViewItemUIs.Add(itemUI);
                }
            }
        }

        Debug.Log($"Se cargaron {spawnedViewItemUIs.Count} items en la vista de inventario");
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

    private void ClearInventoryViewItems()
    {
        foreach (InventoryItemUIDay itemUI in spawnedViewItemUIs)
        {
            if (itemUI != null)
                Destroy(itemUI.gameObject);
        }
        spawnedViewItemUIs.Clear();
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
            SellItem(selectedItem);
        }
        else
        {
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

    public void ShowInventoryViewPanel()
    {
        if (inventoryViewPanel == null) return;

        StartCoroutine(ShowInventoryViewAfterDelay(0.2f));
    }

    private IEnumerator ShowInventoryViewAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        PopulateInventoryView();

        inventoryViewPanel.SetActive(true);
        inventoryViewPanel.transform.localScale = Vector3.zero;
        inventoryViewPanel.transform.DOScale(Vector3.one, popInDuration).SetEase(popInEase);

        if (inventoryViewContent != null)
        {
            yield return new WaitForSeconds(0.1f);
            inventoryViewContent.SetActive(true);
            inventoryViewContent.transform.localScale = Vector3.zero;
            inventoryViewContent.transform.DOScale(Vector3.one, popInDuration * 0.8f).SetEase(popInEase);
        }
    }

    public void HideInventoryViewPanel()
    {
        if (inventoryViewPanel == null) return;

        if (inventoryViewContent != null)
        {
            inventoryViewContent.transform.DOScale(Vector3.zero, popOutDuration * 0.8f)
                .SetEase(popOutEase)
                .OnComplete(() => inventoryViewContent.SetActive(false));
        }

        inventoryViewPanel.transform.DOScale(Vector3.zero, popOutDuration)
            .SetEase(popOutEase)
            .OnComplete(() => {
                inventoryViewPanel.SetActive(false);

                // Mostrar botones principales si no hay interacción activa
                if (!isInteracting)
                {
                    ShowMainButtons();
                }
            });
    }


    #endregion

    #region Button Handlers

    public void OnSellButtonClicked()
    {
        ShowInventorySellPanel();
    }

    public void OnPlayerInventoryClicked()
    {
        HideMainButtons();
        ShowInventoryViewPanel();
    }

    public void OnGoToNightClicked()
    {
        // Aquí iría la lógica para transicionar a la noche
        Debug.Log("Transicionando a la noche...");
    }

    public void OnBackButtonClicked()
    {
        // Determinar qué panel está activo y cerrarlo
        if (inventorySellPanel != null && inventorySellPanel.activeSelf)
        {
            HideInventorySellPanel();
            ShowSelectionsPanel();
        }
        else if (inventoryViewPanel != null && inventoryViewPanel.activeSelf)
        {
            HideInventoryViewPanel();
            if (!isInteracting)
            {
                ShowMainButtons();
            }
        }
    }

    public void OnLeaveButtonClicked()
    {
        StartCoroutine(LeaveCharacterSequence());
    }

    private IEnumerator LeaveCharacterSequence()
    {
        HideSelectionsPanel();
        HideInventorySellPanel();
        HideInventoryViewPanel();

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
        ClearInventoryViewItems();
        isInteracting = false;
        currentInteractingCharacter = null;

        // Asegurarse de que los botones principales estén visibles
        ShowMainButtons();
    }
}