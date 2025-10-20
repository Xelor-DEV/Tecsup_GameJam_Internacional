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

    [Header("Animation Settings")]
    [SerializeField] private float moveDuration = 1f;
    [SerializeField] private Ease moveEase = Ease.InOutCubic;
    [SerializeField] private float scaleDuration = 0.5f;

    [Header("Components")]
    [SerializeField] private Button characterButton;

    private RectTransform characterRect;
    private DayManager dayManager;
    private bool isTalking = false;
    private Vector3 originalScale;
    private Vector3 originalPosition;

    void Awake()
    {
        characterRect = GetComponent<RectTransform>();
        originalScale = transform.localScale;
        originalPosition = transform.position;

        if (characterButton == null)
            characterButton = GetComponentInChildren<Button>();

        SetWaitingState();
    }

    public void Initialize(DayManager manager)
    {
        dayManager = manager;

        if (characterButton != null)
            characterButton.onClick.AddListener(OnCharacterClicked);
    }

    // Nuevo método para obtener los datos del personaje
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

        talkSequence.Append(characterRect.DOMove(dayManager.GetHidePosition().position, moveDuration).SetEase(moveEase));
        talkSequence.AppendCallback(() => SetTalkingState());
        talkSequence.Append(characterRect.DOMove(dayManager.GetTalkPosition().position, moveDuration).SetEase(moveEase));
        talkSequence.Join(characterRect.DOScale(originalScale * 1.1f, scaleDuration).SetLoops(2, LoopType.Yoyo));

        talkSequence.OnComplete(() => {
            if (characterButton != null)
                characterButton.interactable = true;
        });
    }

    public void Leave()
    {
        if (!isTalking) return;

        Sequence leaveSequence = DOTween.Sequence();

        leaveSequence.Append(characterRect.DOMove(dayManager.GetHidePosition().position, moveDuration).SetEase(moveEase));
        leaveSequence.AppendCallback(() => SetWaitingState());
        leaveSequence.Append(characterRect.DOMove(originalPosition, moveDuration).SetEase(moveEase));

        leaveSequence.OnComplete(() => {
            isTalking = false;
        });
    }

    private void SetWaitingState()
    {
        if (waitingState != null) waitingState.SetActive(true);
        if (talkingState != null) talkingState.SetActive(false);
        isTalking = false;
    }

    private void SetTalkingState()
    {
        if (waitingState != null) waitingState.SetActive(false);
        if (talkingState != null) talkingState.SetActive(true);
    }

    void OnDestroy()
    {
        if (characterButton != null)
            characterButton.onClick.RemoveAllListeners();
    }
}