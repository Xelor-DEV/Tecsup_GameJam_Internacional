using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.UI;

public class WindowManager : MonoBehaviour
{
    [Serializable]
    public class WindowConfig
    {
        public RectTransform window;
        public float showDuration = 0.5f;
        public float hideDuration = 0.3f;
        public Ease showEase = Ease.OutBack;
        public Ease hideEase = Ease.InBack;
    }

    [Header("References")]
    [SerializeField] private WindowConfig[] windows;
    [SerializeField] private Button[] buttonsToDisable;
    [SerializeField] private RectTransform showPosition;
    [SerializeField] private RectTransform hidePosition;

    private int currentWindowIndex = -1;

    private void Start()
    {
        InitializeWindows();
    }

    private void InitializeWindows()
    {
        foreach (WindowConfig config in windows)
        {
            if (config.window != null)
            {
                config.window.anchoredPosition = hidePosition.anchoredPosition;
                config.window.gameObject.SetActive(false);
            }
        }
        currentWindowIndex = -1;
        UpdateButtonsInteractability();
    }

    public void ShowWindow(int index)
    {
        if (!IsValidIndex(index)) return;

        if (currentWindowIndex >= 0 && currentWindowIndex != index)
        {
            HideWindow(currentWindowIndex);
        }

        WindowConfig config = windows[index];

        config.window.gameObject.SetActive(true);
        config.window.anchoredPosition = hidePosition.anchoredPosition;

        config.window.DOAnchorPosX(showPosition.anchoredPosition.x, config.showDuration)
            .SetEase(config.showEase);

        currentWindowIndex = index;
        UpdateButtonsInteractability();
    }

    public void HideWindow(int index)
    {
        if (!IsValidIndex(index)) return;

        WindowConfig config = windows[index];

        config.window.DOAnchorPosX(hidePosition.anchoredPosition.x, config.hideDuration)
            .SetEase(config.hideEase)
            .OnComplete(() =>
            {
                config.window.gameObject.SetActive(false);

                if (currentWindowIndex == index)
                {
                    currentWindowIndex = -1;
                }
                UpdateButtonsInteractability();
            });
    }

    public void HideCurrentWindow()
    {
        if (currentWindowIndex >= 0)
        {
            HideWindow(currentWindowIndex);
        }
    }

    public void HideAllWindows()
    {
        foreach (WindowConfig config in windows)
        {
            if (config.window != null && config.window.gameObject.activeSelf)
            {
                config.window.anchoredPosition = hidePosition.anchoredPosition;
                config.window.gameObject.SetActive(false);
            }
        }
        currentWindowIndex = -1;
        UpdateButtonsInteractability();
    }

    private void UpdateButtonsInteractability()
    {
        bool anyWindowActive = currentWindowIndex != -1;

        foreach (Button button in buttonsToDisable)
        {
            if (button != null)
            {
                button.interactable = !anyWindowActive;
            }
        }
    }

    private bool IsValidIndex(int index)
    {
        if (index < 0 || index >= windows.Length)
        {
            Debug.LogError($"Índice de ventana inválido: {index}");
            return false;
        }
        return true;
    }
}