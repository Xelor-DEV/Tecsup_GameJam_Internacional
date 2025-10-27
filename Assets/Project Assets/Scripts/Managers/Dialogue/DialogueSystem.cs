using UnityEngine;
using System.Collections;
using DG.Tweening;
using System;

public class DialogueSystem : NonPersistentSingleton<DialogueSystem>
{
    [Header("References")]
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private CharactersDatabase charactersDatabase;
    [SerializeField] private AudioManager audioManager;

    [Header("Player Voice Settings")]
    [SerializeField] private AudioClip playerVoiceSound;
    [SerializeField] private float playerVoicePitch = 1.0f;
    [SerializeField, Range(0.001f, 0.2f)] private float playerVoiceDelay = 0.08f;

    private DialogueLine[] currentDialogueSequence;
    private int currentDialogueIndex = 0;
    private CharacterData currentCharacter;
    private Action onSequenceComplete;
    private Coroutine voiceCoroutine;
    private bool isPlayerSpeaking = false;
    private bool isDialogueActive = false;
    private bool isPaused = false;
    private float lastSoundTime = 0f;

    public void StartDialogueSequence(string characterName, string dialogueType, Action onComplete = null)
    {
        // Find character in database
        currentCharacter = charactersDatabase.characters.Find(c => c.characterName == characterName);
        if (currentCharacter == null)
        {
            Debug.LogError($"Character {characterName} not found in database");
            onComplete?.Invoke();
            return;
        }

        // Get dialogue data from character
        var characterDialogueData = currentCharacter.dialogueData;
        if (characterDialogueData == null)
        {
            Debug.LogError($"No dialogue data found for character {characterName}");
            onComplete?.Invoke();
            return;
        }

        // Get the appropriate dialogue sequence
        switch (dialogueType)
        {
            case "Speak":
                currentDialogueSequence = characterDialogueData.speakDialogue;
                break;
            case "Leave":
                currentDialogueSequence = characterDialogueData.leaveDialogue;
                break;
            case "WrongItem":
                currentDialogueSequence = characterDialogueData.wrongItemDialogue;
                break;
            case "CorrectItem":
                currentDialogueSequence = characterDialogueData.correctItemDialogue;
                break;
            case "SellItem":
                currentDialogueSequence = characterDialogueData.sellItemDialogue;
                break;
            default:
                Debug.LogError($"Unknown dialogue type: {dialogueType}");
                onComplete?.Invoke();
                return;
        }

        if (currentDialogueSequence == null || currentDialogueSequence.Length == 0)
        {
            Debug.LogWarning($"No dialogue sequence found for {dialogueType} on character {characterName}");
            onComplete?.Invoke();
            return;
        }

        onSequenceComplete = onComplete;
        currentDialogueIndex = 0;
        isDialogueActive = true;
        isPaused = false;
        lastSoundTime = 0f;
        ShowNextDialogueLine();
    }

    private void ShowNextDialogueLine()
    {
        if (currentDialogueIndex >= currentDialogueSequence.Length || !isDialogueActive)
        {
            // Sequence complete
            StopAllAnimationsAndSounds();
            onSequenceComplete?.Invoke();
            isDialogueActive = false;
            return;
        }

        var currentLine = currentDialogueSequence[currentDialogueIndex];
        isPlayerSpeaking = currentLine.isPlayerSpeaking;

        // Detener sonidos y animaciones previos
        StopVoiceSound();
        var characterUI = UIManager.Instance.GetCurrentInteractingCharacter();
        if (characterUI != null) characterUI.StopTalkingAnimation();

        if (isPlayerSpeaking)
        {
            // Player speaking
            dialogueUI.ShowPlayerDialogue(currentLine.dialogueText, OnDialogueLineComplete);
            StartVoiceSound(true); // true for player
        }
        else
        {
            // NPC speaking
            characterUI?.StartTalkingAnimation();
            dialogueUI.ShowNPCDialogue(currentCharacter.characterName, currentLine.dialogueText, OnDialogueLineComplete);
            StartVoiceSound(false); // false for NPC
        }
    }

    private void OnDialogueLineComplete()
    {
        StopVoiceSound();

        var characterUI = UIManager.Instance.GetCurrentInteractingCharacter();
        characterUI?.StopTalkingAnimation();

        currentDialogueIndex++;
        ShowNextDialogueLine();
    }

    private void StartVoiceSound(bool isPlayer)
    {
        if (voiceCoroutine != null)
            StopCoroutine(voiceCoroutine);

        AudioClip voiceClip = isPlayer ? playerVoiceSound : (currentCharacter != null ? currentCharacter.voiceSound : null);
        float pitch = isPlayer ? playerVoicePitch : (currentCharacter != null ? currentCharacter.voicePitch : 1.0f);
        float delay = isPlayer ? playerVoiceDelay : dialogueUI.VoiceSoundDelay;

        if (voiceClip != null && audioManager != null)
        {
            // Reproducir primer sonido inmediatamente
            PlayVoiceSoundImmediately(voiceClip, pitch, isPlayer);

            // Iniciar coroutine para sonidos posteriores
            voiceCoroutine = StartCoroutine(PlayVoiceSoundRoutine(voiceClip, pitch, delay, isPlayer));
        }
    }

    private void PlayVoiceSoundImmediately(AudioClip voiceClip, float pitch, bool isPlayer)
    {
        int audioSourceIndex = isPlayer ? 2 : 1;
        var audioSource = audioManager.GetAudioSourceByIndex(audioSourceIndex);

        if (audioSource != null && audioSource.isActiveAndEnabled)
        {
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(voiceClip);

            // Sincronizar animación con el sonido para NPC
            var characterUI = UIManager.Instance.GetCurrentInteractingCharacter();
            if (characterUI != null && !isPlayer)
            {
                characterUI.PulseTalkingAnimation();
            }
        }

        lastSoundTime = Time.time;
    }

    private IEnumerator PlayVoiceSoundRoutine(AudioClip voiceClip, float pitch, float baseDelay, bool isPlayer)
    {
        var dialogueUIInstance = dialogueUI;
        if (dialogueUIInstance == null) yield break;

        while (dialogueUIInstance.IsTyping() && isDialogueActive && !isPaused)
        {
            // Verificar si ha pasado suficiente tiempo desde el último sonido
            if (Time.time - lastSoundTime >= baseDelay)
            {
                // Get audio source - player usa índice 2, NPC usa índice 1
                int audioSourceIndex = isPlayer ? 2 : 1;
                var audioSource = audioManager.GetAudioSourceByIndex(audioSourceIndex);

                if (audioSource != null && audioSource.isActiveAndEnabled)
                {
                    audioSource.pitch = pitch;
                    audioSource.PlayOneShot(voiceClip);

                    // Get character UI to sync animation with sound
                    var characterUI = UIManager.Instance.GetCurrentInteractingCharacter();
                    if (characterUI != null && !isPlayer)
                    {
                        characterUI.PulseTalkingAnimation();
                    }

                    lastSoundTime = Time.time;
                }
            }

            // Esperar un frame en lugar de un tiempo fijo para mejor respuesta
            yield return null;
        }
    }

    private void StopVoiceSound()
    {
        if (voiceCoroutine != null)
        {
            StopCoroutine(voiceCoroutine);
            voiceCoroutine = null;
        }

        // También detener los sonidos en los AudioSources específicos
        if (audioManager != null)
        {
            var npcAudioSource = audioManager.GetAudioSourceByIndex(1);
            var playerAudioSource = audioManager.GetAudioSourceByIndex(2);
            if (npcAudioSource != null) npcAudioSource.Stop();
            if (playerAudioSource != null) playerAudioSource.Stop();
        }
    }

    private void StopAllAnimationsAndSounds()
    {
        StopVoiceSound();
        isDialogueActive = false;
        isPaused = false;

        var characterUI = UIManager.Instance.GetCurrentInteractingCharacter();
        characterUI?.StopTalkingAnimation();
    }

    public void ForceEndDialogue()
    {
        StopAllAnimationsAndSounds();
        dialogueUI.HideAllDialogs();
        currentDialogueSequence = null;
        currentDialogueIndex = 0;
        onSequenceComplete?.Invoke();
    }

    public void CompleteCurrentLine()
    {
        dialogueUI.CompleteTypewriter();
    }

    // Called when typewriter effect is completed (naturally or by click)
    public void OnTypewriterCompleted()
    {
        StopVoiceSound();

        var characterUI = UIManager.Instance.GetCurrentInteractingCharacter();
        characterUI?.StopTalkingAnimation();
    }

    public void OnTypewriterPaused()
    {
        isPaused = true;
        StopVoiceSound();

        var characterUI = UIManager.Instance.GetCurrentInteractingCharacter();
        characterUI?.PauseTalkingAnimation();
    }

    public void OnTypewriterResumed()
    {
        isPaused = false;

        var characterUI = UIManager.Instance.GetCurrentInteractingCharacter();
        characterUI?.ResumeTalkingAnimation();

        // Reiniciar el sonido si todavía hay texto escribiéndose
        if (dialogueUI.IsTyping() && isDialogueActive)
        {
            StartVoiceSound(isPlayerSpeaking);
        }
    }
}