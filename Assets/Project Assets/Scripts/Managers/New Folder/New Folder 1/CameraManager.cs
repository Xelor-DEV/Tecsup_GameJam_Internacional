using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;

public class CameraManager : NonPersistentSingleton<CameraManager>
{
    [Header("Configuración de Cámaras")]
    [SerializeField] private CinemachineCamera[] cameras;
    [SerializeField] private int defaultCameraIndex = 0;

    [Header("Configuración de Fade")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    private int currentCameraIndex;
    private bool isTransitioning = false;

    // Propiedad pública para verificar si hay una transición en curso
    public static bool IsTransitioning { get; private set; }

    private void Start()
    {
        InitializeCameras();
        StartCoroutine(StartSequence());
    }

    private void InitializeCameras()
    {
        if (cameras.Length == 0)
        {
            Debug.LogError("No hay cámaras asignadas en el CameraManager!");
            return;
        }

        foreach (var cam in cameras)
        {
            cam.Priority = 0;
        }

        currentCameraIndex = Mathf.Clamp(defaultCameraIndex, 0, cameras.Length - 1);
        cameras[currentCameraIndex].Priority = 10;
    }

    public void SwitchCamera(int newCameraIndex)
    {
        if (isTransitioning || newCameraIndex == currentCameraIndex)
            return;

        cameras[currentCameraIndex].Priority = 0;
        currentCameraIndex = newCameraIndex;
        cameras[currentCameraIndex].Priority = 10;
    }

    private IEnumerator CameraTransitionSequence(int newCameraIndex)
    {
        isTransitioning = true;
        IsTransitioning = true;

        // Fade In
        yield return StartCoroutine(Fade(0f, 1f));

        // Cambiar cámara
        cameras[currentCameraIndex].Priority = 0;
        currentCameraIndex = newCameraIndex;
        cameras[currentCameraIndex].Priority = 10;

        // Fade Out
        yield return StartCoroutine(Fade(1f, 0f));

        isTransitioning = false;
        IsTransitioning = false;
    }

    public IEnumerator Fade(float startAlpha, float targetAlpha)
    {
        float elapsedTime = 0f;
        Color color = fadeImage.color;
        color.a = startAlpha;
        fadeImage.color = color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = targetAlpha;
        fadeImage.color = color;
    }

    private IEnumerator StartSequence()
    {
        Time.timeScale = 0f;
        IsTransitioning = true;
        fadeImage.color = Color.black;

        yield return StartCoroutine(Fade(1f, 0f));

        Time.timeScale = 1f;
        IsTransitioning = false;
    }
}