using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

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

    private Coroutine loadingCoroutine;
    private bool isAnimating = false;

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