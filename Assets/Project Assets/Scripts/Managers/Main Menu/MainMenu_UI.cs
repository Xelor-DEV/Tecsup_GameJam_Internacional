using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using UnityEngine.InputSystem;
using TMPro;

public class MainMenu_UI : MonoBehaviour
{
    [Header("Loading Screen References")]
    [SerializeField] private RectTransform loadingWindow;
    [SerializeField] private Image loadingBar;
    [Header("Settings")]
    [SerializeField] private string gameScene;
    [Header("DOTween Animation Settings")]
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private Ease easeType = Ease.OutBack;
    [Header("Character References")]
    [SerializeField] private RectTransform player;
    [SerializeField] private RectTransform kids;
    [Header("Hide Positions")]
    [SerializeField] private RectTransform playerHidePosition;
    [SerializeField] private RectTransform kidsHidePosition;
    [SerializeField] private RectTransform selectionMenuHidePosition;
    [Header("Show Positions")]
    [SerializeField] private RectTransform selectionMenuShowPosition;
    [Header("Selection Menu")]
    [SerializeField] private RectTransform selectionMenu;
    [Header("Individual Animation Durations")]
    [SerializeField] private float playerAnimationDuration = 0.5f;
    [SerializeField] private float kidsAnimationDuration = 0.5f;
    [SerializeField] private float menuAnimationDuration = 0.5f;
    [SerializeField] private Ease easeOther = Ease.Linear;
    [SerializeField] private TMP_Text message;

    private Coroutine loadingCoroutine;
    private bool isAnimating = false;
    private bool isSelectionMenuVisible = false;

    private void Start()
    {
        // Inicializar posiciones
        selectionMenu.position = selectionMenuHidePosition.position;
    }

    public void OnToggleSelectionMenu(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ToggleSelectionMenu();
        }
    }

    private void ToggleSelectionMenu()
    {
        if (isAnimating) return;

        if (!isSelectionMenuVisible)
        {
            ShowSelectionMenu();
        }
        // Removido el else para que el menú no se oculte una vez mostrado
    }

    private void ShowSelectionMenu()
    {
        isAnimating = true;

        // Ocultar personajes con duraciones individuales
        player.DOMove(playerHidePosition.position, playerAnimationDuration).SetEase(easeOther);
        kids.DOMove(kidsHidePosition.position, kidsAnimationDuration).SetEase(easeOther)
            .OnComplete(() =>
            {

            });
        // Mostrar menú de selección con duración específica
        selectionMenu.DOMove(selectionMenuShowPosition.position, menuAnimationDuration)
            .SetEase(easeType)
            .OnComplete(() =>
            {
                isAnimating = false;
                isSelectionMenuVisible = true;
            });
        message.text = string.Empty;
    }

    public void LoadGameScene()
    {
        if (isAnimating || loadingCoroutine != null)
            return;

        isAnimating = true;

        loadingWindow.gameObject.SetActive(true);
        loadingBar.fillAmount = 0f;

        if (loadingWindow != null)
        {
            loadingWindow.localScale = Vector3.zero;

            loadingWindow.DOScale(Vector3.one, animationDuration)
                .SetEase(easeType)
                .OnComplete(() => {
                    isAnimating = false;
                    loadingCoroutine = StartCoroutine(LoadGameSceneCoroutine());
                });
        }
        else
        {
            isAnimating = false;
            loadingCoroutine = StartCoroutine(LoadGameSceneCoroutine());
        }
    }

    private IEnumerator LoadGameSceneCoroutine()
    {
        AsyncOperation asyncOp = GlobalSceneManager.Instance.LoadSceneAsyncWithoutActivation(gameScene, false);

        while (!asyncOp.isDone)
        {
            float progress = Mathf.Clamp01(asyncOp.progress / 0.9f);
            loadingBar.fillAmount = progress;

            if (asyncOp.progress >= 0.9f)
            {
                asyncOp.allowSceneActivation = true;
            }

            yield return null;
        }

        loadingCoroutine = null;
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}