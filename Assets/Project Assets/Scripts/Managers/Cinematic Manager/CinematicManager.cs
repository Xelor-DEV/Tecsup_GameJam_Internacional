using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class CinematicManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private GameObject loadingObject;

    [Header("Scene configuration")]
    [SerializeField] private string nextSceneName;

    private AsyncOperation sceneAsyncOp;
    private bool isLoadingScene = false;

    private void OnEnable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoEnd;
        }
    }

    private void OnDisable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEnd;
        }
    }

    private void Awake()
    {
        if (videoPlayer != null)
        {
            loadingObject?.SetActive(false);
        }
    }

    public void GetSkipCinematic(InputAction.CallbackContext callback)
    {
        if (callback.performed && !isLoadingScene)
        {
            SkipCinematic();
        }
    }

    public void SkipCinematic()
    {
        if (isLoadingScene) return;

        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }

        StartCoroutine(LoadNextScene());
    }

    private void OnVideoEnd(VideoPlayer source)
    {
        if (!isLoadingScene)
        {
            StartCoroutine(LoadNextScene());
        }
    }

    private IEnumerator LoadNextScene()
    {
        isLoadingScene = true;

        if (loadingObject != null)
        {
            loadingObject.SetActive(true);
        }
            
        sceneAsyncOp = SceneManager.LoadSceneAsync(nextSceneName);
        sceneAsyncOp.allowSceneActivation = false;

        while (sceneAsyncOp.progress < 0.9f)
        {
            yield return null;
        }

        sceneAsyncOp.allowSceneActivation = true;
    }
}