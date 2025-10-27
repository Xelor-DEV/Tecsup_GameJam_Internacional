using UnityEngine;
using TMPro;
using System;
using DG.Tweening;
using System.Collections;

public class DialogueUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject playerDialogPanel;
    [SerializeField] private TMP_Text playerDialogText;
    [SerializeField] private GameObject npcDialogPanel;
    [SerializeField] private TMP_Text npcDialogText;
    [SerializeField] private TMP_Text npcNameText;

    [Header("Tween Settings")]
    [SerializeField] private float popInDuration = 0.3f;
    [SerializeField] private float popOutDuration = 0.2f;
    [SerializeField] private Ease popInEase = Ease.OutBack;
    [SerializeField] private Ease popOutEase = Ease.InBack;

    [Header("Typewriter Settings")]
    [SerializeField, Range(10, 100)] private float charsPerSecond = 40f;
    [SerializeField, Range(0.1f, 1.0f)] private float punctuationDelay = 0.3f;
    [SerializeField, Range(0.01f, 0.2f)] private float voiceSoundDelay = 0.08f;

    private System.Action onDialogueComplete;
    private Coroutine currentTypewriterCoroutine;
    private bool isTyping = false;
    private string currentFullText = "";

    public float CharsPerSecond => charsPerSecond;
    public float PunctuationDelay => punctuationDelay;
    public float VoiceSoundDelay => voiceSoundDelay;

    public void ShowPlayerDialogue(string text, Action onComplete = null)
    {
        onDialogueComplete = onComplete;

        if (npcDialogPanel.activeSelf)
        {
            npcDialogPanel.transform.DOScale(Vector3.zero, popOutDuration)
                .SetEase(popOutEase)
                .OnComplete(() =>
                {
                    npcDialogPanel.SetActive(false);
                    ShowPlayerDialogInternal(text);
                });
        }
        else
        {
            ShowPlayerDialogInternal(text);
        }
    }




    public void ShowNPCDialogue(string characterName, string text, System.Action onComplete = null)
    {
        onDialogueComplete = onComplete;

        if (playerDialogPanel.activeSelf)
        {
            playerDialogPanel.transform.DOScale(Vector3.zero, popOutDuration)
                .SetEase(popOutEase)
                .OnComplete(() =>
                {
                    playerDialogPanel.SetActive(false);
                    ShowNPCDialogInternal(characterName, text);
                });
        }
        else
        {
            ShowNPCDialogInternal(characterName, text);
        }
    }

    private void ShowPlayerDialogInternal(string text)
    {
        playerDialogText.text = "";
        playerDialogPanel.SetActive(true);
        playerDialogPanel.transform.localScale = Vector3.zero;
        playerDialogPanel.transform.DOScale(Vector3.one, popInDuration).SetEase(popInEase);

        StartTypewriterEffect(playerDialogText, text);
    }

    private void ShowNPCDialogInternal(string characterName, string text)
    {
        npcNameText.text = characterName;
        npcDialogText.text = "";
        npcDialogPanel.SetActive(true);
        npcDialogPanel.transform.localScale = Vector3.zero;
        npcDialogPanel.transform.DOScale(Vector3.one, popInDuration).SetEase(popInEase);

        StartTypewriterEffect(npcDialogText, text);
    }

    private void StartTypewriterEffect(TMP_Text textComponent, string text)
    {
        if (currentTypewriterCoroutine != null)
            StopCoroutine(currentTypewriterCoroutine);

        currentFullText = text;
        currentTypewriterCoroutine = StartCoroutine(TypewriterEffect(textComponent, text));
    }

    private IEnumerator TypewriterEffect(TMP_Text textComponent, string text)
    {
        isTyping = true;
        textComponent.text = "";

        for (int i = 0; i < text.Length; i++)
        {
            textComponent.text += text[i];

            // Check for punctuation and add delay
            if (i < text.Length - 1 && IsPunctuation(text[i]))
            {
                // Notify dialogue system to pause animations during punctuation
                DialogueSystem.Instance.OnTypewriterPaused();
                yield return new WaitForSeconds(punctuationDelay);
                DialogueSystem.Instance.OnTypewriterResumed();
            }
            else
            {
                yield return new WaitForSeconds(1f / charsPerSecond);
            }
        }

        isTyping = false;
        // Notify dialogue system that typing is complete
        DialogueSystem.Instance.OnTypewriterCompleted();
    }

    private bool IsPunctuation(char character)
    {
        // Incluir puntos suspensivos (tres puntos consecutivos)
        return character == '.' || character == ',' || character == '!' ||
               character == '?' || character == ';' || character == ':' ||
               character == '…' || character == '¡' || character == '¿';
    }

    public void CompleteTypewriter()
    {
        if (currentTypewriterCoroutine != null && isTyping)
        {
            StopCoroutine(currentTypewriterCoroutine);
            isTyping = false;

            // Set the full text immediately
            if (playerDialogPanel.activeSelf)
            {
                playerDialogText.text = currentFullText;
            }
            else if (npcDialogPanel.activeSelf)
            {
                npcDialogText.text = currentFullText;
            }

            // Notify dialogue system that typing is complete
            DialogueSystem.Instance.OnTypewriterCompleted();
        }
    }

    // Called by the Next button in both dialog panels
    public void OnNextDialogue()
    {
        if (isTyping)
        {
            CompleteTypewriter();
        }
        else
        {
            onDialogueComplete?.Invoke();
            HideAllDialogs();
        }
    }

    public void HideAllDialogs()
    {
        if (playerDialogPanel.activeSelf)
        {
            playerDialogPanel.transform.DOScale(Vector3.zero, popOutDuration)
                .SetEase(popOutEase)
                .OnComplete(() => playerDialogPanel.SetActive(false));
        }

        if (npcDialogPanel.activeSelf)
        {
            npcDialogPanel.transform.DOScale(Vector3.zero, popOutDuration)
                .SetEase(popOutEase)
                .OnComplete(() => npcDialogPanel.SetActive(false));
        }

        if (currentTypewriterCoroutine != null)
        {
            StopCoroutine(currentTypewriterCoroutine);
            isTyping = false;
        }
    }

    public bool IsTyping()
    {
        return isTyping;
    }
}