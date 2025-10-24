using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;

public class SelectedButtonTween : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Tween Settings")]
    [SerializeField] private float highlightedScale = 1.1f;
    [SerializeField] private float pressedScale = 0.9f;
    [SerializeField] private float scaleDuration = 0.2f;
    [SerializeField] private Ease easeType = Ease.OutBack;

    private Vector3 originalScale;
    private Tween currentTween;
    private bool isHighlighted = false;
    private Button button;
    private RectTransform scaleContainer; // Nuevo contenedor para la escala

    private void Awake()
    {
        button = GetComponent<Button>();
        CreateScaleContainer();
        originalScale = scaleContainer.localScale;
    }

    private void CreateScaleContainer()
    {
        // Crear un objeto hijo para manejar la escala
        GameObject containerGO = new GameObject("ScaleContainer");
        scaleContainer = containerGO.AddComponent<RectTransform>();
        scaleContainer.SetParent(transform);

        // Configurar el contenedor para que ocupe todo el espacio del botón
        scaleContainer.localPosition = Vector3.zero;
        scaleContainer.localScale = Vector3.one;
        scaleContainer.anchorMin = Vector2.zero;
        scaleContainer.anchorMax = Vector2.one;
        scaleContainer.offsetMin = Vector2.zero;
        scaleContainer.offsetMax = Vector2.zero;

        // Mover todos los hijos al nuevo contenedor
        while (transform.childCount > 0)
        {
            Transform child = transform.GetChild(0);
            child.SetParent(scaleContainer);
        }
    }

    private void OnDisable()
    {
        currentTween?.Kill();
        scaleContainer.localScale = originalScale;
        isHighlighted = false;
    }

    private bool IsButtonInteractable()
    {
        return button == null || button.interactable;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsButtonInteractable()) return;

        isHighlighted = true;
        currentTween?.Kill();
        currentTween = scaleContainer.DOScale(originalScale * highlightedScale, scaleDuration).SetEase(easeType);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!IsButtonInteractable()) return;

        isHighlighted = false;
        currentTween?.Kill();
        currentTween = scaleContainer.DOScale(originalScale, scaleDuration).SetEase(easeType);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!IsButtonInteractable()) return;

        currentTween?.Kill();
        currentTween = scaleContainer.DOScale(originalScale * pressedScale, scaleDuration * 0.5f).SetEase(Ease.OutQuad);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!IsButtonInteractable()) return;

        currentTween?.Kill();

        Vector3 targetScale = isHighlighted ?
            originalScale * highlightedScale :
            originalScale;

        currentTween = scaleContainer.DOScale(targetScale, scaleDuration).SetEase(easeType);
    }
}