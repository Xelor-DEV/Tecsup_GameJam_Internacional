using UnityEngine;
using System;

[CreateAssetMenu(fileName = "DialogueData", menuName = "Game/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [Header("Dialogue Sequences")]
    public DialogueLine[] speakDialogue;
    public DialogueLine[] leaveDialogue;
    public DialogueLine[] wrongItemDialogue;
    public DialogueLine[] correctItemDialogue;
    public DialogueLine[] sellItemDialogue;
}

[Serializable]
public class DialogueLine
{
    public bool isPlayerSpeaking;
    [TextArea(3, 5)]
    public string dialogueText;
}