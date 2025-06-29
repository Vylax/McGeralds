using UnityEngine;

public class NPCTrigger : MonoBehaviour
{
    [Header("NPC Settings")]
    public DynamicSpeechBubble npcToTalkTo;
    [Tooltip("If true, uses the custom dialogue line below. If false, uses random dialogue from preset lines.")]
    public bool useCustomDialogue = false;
    [Tooltip("Custom dialogue line to use when useCustomDialogue is true.")]
    public string dialogueLine;
    public AudioClip voiceLine;
}