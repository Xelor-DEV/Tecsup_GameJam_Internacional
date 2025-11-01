using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Game/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Basic Info")]
    public string characterName;
    public int[] availableDays;
    public bool isOptional;
    public GameObject characterPrefab;

    [Header("Dialogue Data")]
    public DialogueData dialogueData;

    [Header("Voice Settings")]
    public float voicePitch = 1.0f;
    public AudioClip voiceSound;

    [Header("Animation Settings")]
    public bool useJawMovement = true;
    public bool useScaleEffect = false;
    public bool useEyeScaleEffect = false;

    [Range(1f, 150f)]
    public float jawMovementAmount = 10f; // Ahora es valor absoluto en unidades

    [Range(0.1f, 2.0f)]
    public float animationSpeed = 0.8f;

    [Range(1.0f, 3.0f)]
    public float eyeScaleAmount = 1.5f;
}