using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CharacterUI : MonoBehaviour
{
    [Header("Character Data")]
    [SerializeField] private CharacterData characterData;

    [Header("Character States")]
    [SerializeField] private GameObject waitingState;
    [SerializeField] private GameObject talkingState;

    [Header("Animation References")]
    [SerializeField] private RectTransform jawTransform;
    [SerializeField] private RectTransform eyeTransform;

    [Header("Components")]
    [SerializeField] private Button characterButton;

    private RectTransform characterRect;
    private DayManager dayManager;
    private bool isTalking = false;
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Transform originalParent;
    private Vector2 jawOriginalPosition;
    private Vector3 eyeOriginalScale;

    // Animation sequences
    private Sequence jawSequence;
    private Sequence scaleSequence;
    private Sequence eyeSequence;
    private bool isAnimating = false;
    private bool isAnimationPaused = false;

    void Awake()
    {
        characterRect = GetComponent<RectTransform>();
        originalScale = transform.localScale;
        originalPosition = transform.position;
        originalParent = transform.parent;

        if (characterButton != null)
            characterButton = GetComponentInChildren<Button>();

        if (jawTransform != null)
            jawOriginalPosition = jawTransform.anchoredPosition;

        if (eyeTransform != null)
            eyeOriginalScale = eyeTransform.localScale;

        SetWaitingState();
    }

    public void Initialize(DayManager manager)
    {
        dayManager = manager;

        if (characterButton != null)
            characterButton.onClick.AddListener(OnCharacterClicked);
    }

    public CharacterData GetCharacterData()
    {
        return characterData;
    }

    private void OnCharacterClicked()
    {
        if (isTalking || !UIManager.Instance.CanInteractWithCharacter()) return;
        StartTalkingSequence();
    }

    private void StartTalkingSequence()
    {
        isTalking = true;
        UIManager.Instance.StartCharacterInteraction(this);

        if (characterButton != null)
            characterButton.interactable = false;

        Sequence talkSequence = DOTween.Sequence();

        // Primero movemos al personaje a la posición Hide
        talkSequence.Append(characterRect.DOMove(dayManager.GetHidePosition().position, 1f).SetEase(Ease.InOutCubic));

        // Cuando llega a Hide, lo hacemos hijo del Hide position
        talkSequence.AppendCallback(() => {
            transform.SetParent(dayManager.GetHidePosition());
            SetTalkingState();
        });

        // Luego lo movemos a Talk position (sin cambiar el parent)
        talkSequence.Append(characterRect.DOMove(dayManager.GetTalkPosition().position, 1f).SetEase(Ease.InOutCubic));
        talkSequence.Join(characterRect.DOScale(originalScale * 1.1f, 0.5f).SetLoops(2, LoopType.Yoyo));

        talkSequence.OnComplete(() => {
            if (characterButton != null)
                characterButton.interactable = true;
        });
    }

    public void StartTalkingAnimation()
    {
        if (characterData == null) return;

        isAnimating = true;
        isAnimationPaused = false;
        float animSpeed = characterData.animationSpeed;

        // Animación de mandíbula - valor absoluto en lugar de multiplicador
        if (characterData.useJawMovement && jawTransform != null)
        {
            jawSequence = DOTween.Sequence();
            // Usamos valor absoluto para el movimiento
            jawSequence.Append(jawTransform.DOAnchorPosY(
                jawOriginalPosition.y - characterData.jawMovementAmount,
                animSpeed * 0.8f).SetEase(Ease.InOutSine));
            jawSequence.Append(jawTransform.DOAnchorPosY(
                jawOriginalPosition.y,
                animSpeed * 0.8f).SetEase(Ease.InOutSine));
            jawSequence.SetLoops(-1, LoopType.Restart);
        }

        // Animación de escala global del personaje - puede coexistir con jaw movement
        if (characterData.useScaleEffect)
        {
            scaleSequence = DOTween.Sequence();
            scaleSequence.Append(transform.DOScale(originalScale * 1.03f, animSpeed * 0.6f).SetEase(Ease.InOutSine));
            scaleSequence.Append(transform.DOScale(originalScale, animSpeed * 0.6f).SetEase(Ease.InOutSine));
            scaleSequence.SetLoops(-1, LoopType.Restart);
        }

        // Animación de escala en Y del ojo - solo si jaw movement está desactivado
        if (characterData.useEyeScaleEffect && eyeTransform != null && !characterData.useJawMovement)
        {
            eyeSequence = DOTween.Sequence();
            eyeSequence.Append(eyeTransform.DOScaleY(
                eyeOriginalScale.y * characterData.eyeScaleAmount,
                animSpeed * 0.7f).SetEase(Ease.InOutSine));
            eyeSequence.Append(eyeTransform.DOScaleY(
                eyeOriginalScale.y,
                animSpeed * 0.7f).SetEase(Ease.InOutSine));
            eyeSequence.SetLoops(-1, LoopType.Restart);
        }
    }

    public void PulseTalkingAnimation()
    {
        // Pequeño pulso sincronizado con el sonido
        if (!isAnimating || characterData == null || isAnimationPaused) return;

        float quickAnimSpeed = 0.15f;

        // Quick jaw pulse - solo si está activo
        if (characterData.useJawMovement && jawTransform != null)
        {
            // Detener cualquier animación de pulso previa
            jawTransform.DOKill(true);

            // Guardar la posición actual para restaurarla después
            Vector2 currentPos = jawTransform.anchoredPosition;

            jawTransform.DOAnchorPosY(
                jawOriginalPosition.y - characterData.jawMovementAmount * 0.7f,
                quickAnimSpeed).SetEase(Ease.OutSine)
                .OnComplete(() => jawTransform.DOAnchorPosY(jawOriginalPosition.y, quickAnimSpeed).SetEase(Ease.InSine));
        }

        // Quick eye scale pulse - solo si está activo y jaw movement desactivado
        if (characterData.useEyeScaleEffect && eyeTransform != null && !characterData.useJawMovement)
        {
            // Detener cualquier animación de pulso previa
            eyeTransform.DOKill(true);

            eyeTransform.DOScaleY(
                eyeOriginalScale.y * (characterData.eyeScaleAmount * 0.8f),
                quickAnimSpeed).SetEase(Ease.OutSine)
                .OnComplete(() => eyeTransform.DOScaleY(eyeOriginalScale.y, quickAnimSpeed).SetEase(Ease.InSine));
        }
    }

    public void StopTalkingAnimation()
    {
        isAnimating = false;
        isAnimationPaused = false;

        // Detener animación de mandíbula
        if (jawSequence != null)
        {
            jawSequence.Kill();
            jawSequence = null;
        }

        // Detener animación de escala
        if (scaleSequence != null)
        {
            scaleSequence.Kill();
            scaleSequence = null;
        }

        // Detener animación del ojo
        if (eyeSequence != null)
        {
            eyeSequence.Kill();
            eyeSequence = null;
        }

        // Restaurar posiciones y escalas originales
        if (jawTransform != null)
            jawTransform.anchoredPosition = jawOriginalPosition;

        transform.localScale = originalScale;

        if (eyeTransform != null)
            eyeTransform.localScale = eyeOriginalScale;
    }

    public void PauseTalkingAnimation()
    {
        isAnimationPaused = true;

        if (jawSequence != null)
        {
            jawSequence.Pause();
            // Restaurar posición de mandíbula durante pausa
            if (jawTransform != null)
                jawTransform.anchoredPosition = jawOriginalPosition;
        }
        if (scaleSequence != null)
        {
            scaleSequence.Pause();
            // Restaurar escala durante pausa
            transform.localScale = originalScale;
        }
        if (eyeSequence != null)
        {
            eyeSequence.Pause();
            // Restaurar escala del ojo durante pausa
            if (eyeTransform != null)
                eyeTransform.localScale = eyeOriginalScale;
        }
    }

    public void ResumeTalkingAnimation()
    {
        isAnimationPaused = false;

        if (jawSequence != null) jawSequence.Play();
        if (scaleSequence != null) scaleSequence.Play();
        if (eyeSequence != null) eyeSequence.Play();
    }

    public void Leave()
    {
        if (!isTalking) return;

        StopTalkingAnimation();

        Sequence leaveSequence = DOTween.Sequence();

        // Primero movemos de vuelta a Hide position
        leaveSequence.Append(characterRect.DOMove(dayManager.GetHidePosition().position, 1f).SetEase(Ease.InOutCubic));

        // Cuando llega a Hide, quitamos el parent y restauramos el parent original
        leaveSequence.AppendCallback(() => {
            transform.SetParent(originalParent);
            SetWaitingState();
        });

        // Finalmente movemos a la posición original
        leaveSequence.Append(characterRect.DOMove(originalPosition, 1f).SetEase(Ease.InOutCubic));

        leaveSequence.OnComplete(() => {
            isTalking = false;
        });
    }

    private void SetWaitingState()
    {
        if (waitingState != null) waitingState.SetActive(true);
        if (talkingState != null) talkingState.SetActive(false);
        isTalking = false;
        StopTalkingAnimation();
    }

    private void SetTalkingState()
    {
        if (waitingState != null) waitingState.SetActive(false);
        if (talkingState != null) talkingState.SetActive(true);
    }

    void OnDestroy()
    {
        StopTalkingAnimation();

        if (characterButton != null)
            characterButton.onClick.RemoveAllListeners();
    }
}