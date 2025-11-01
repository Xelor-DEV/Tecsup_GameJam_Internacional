using UnityEngine;
using DG.Tweening;
using System.Collections;

public class LogoAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    public bool animateOnStart = true;
    public float interval = 2f;

    [Header("Scale Animation")]
    public bool enableScaleAnimation = true;
    public float scaleMultiplier = 1.2f;
    public float scaleDuration = 0.5f;
    public Ease scaleEase = Ease.OutBack;

    [Header("Rotation Animation")]
    public bool enableRotationAnimation = true;
    public float rotationAngle = 15f;
    public float rotationDuration = 0.8f;
    public Ease rotationEase = Ease.InOutCubic;

    private Vector3 originalScale;
    private Vector3 originalRotation;
    private Sequence animationSequence;

    private void Awake()
    {
        originalScale = transform.localScale;
        originalRotation = transform.eulerAngles;

        if (animateOnStart)
        {
            StartAnimation();
        }
    }

    public void StartAnimation()
    {
        StopAnimation();
        StartCoroutine(AnimationRoutine());
    }

    public void StopAnimation()
    {
        if (animationSequence != null && animationSequence.IsActive())
        {
            animationSequence.Kill();
        }
        StopAllCoroutines();
        ResetToOriginal();
    }

    private IEnumerator AnimationRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            ExecuteAnimations();
        }
    }

    private void ExecuteAnimations()
    {
        animationSequence = DOTween.Sequence();

        // Animación de escala
        if (enableScaleAnimation)
        {
            animationSequence.Join(transform.DOScale(originalScale * scaleMultiplier, scaleDuration)
                .SetEase(scaleEase)
                .OnComplete(() => transform.DOScale(originalScale, scaleDuration).SetEase(scaleEase)));
        }

        // Animación de rotación
        if (enableRotationAnimation)
        {
            animationSequence.Join(transform.DORotate(new Vector3(0, 0, rotationAngle), rotationDuration / 2)
                .SetEase(rotationEase)
                .OnComplete(() => transform.DORotate(new Vector3(0, 0, -rotationAngle), rotationDuration)
                .SetEase(rotationEase)
                .OnComplete(() => transform.DORotate(originalRotation, rotationDuration / 2).SetEase(rotationEase))));
        }
    }

    private void ResetToOriginal()
    {
        transform.localScale = originalScale;
        transform.eulerAngles = originalRotation;
    }

    void OnDestroy()
    {
        StopAnimation();
    }
}