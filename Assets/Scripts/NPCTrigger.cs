using UnityEngine;
using UnityEngine.InputSystem;

public class NPCTrigger : MonoBehaviour
{
    public DynamicSpeechBubble npcToTalkTo;
    public string dialogueLine;
    public AudioClip voiceLine;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            npcToTalkTo.Talk(dialogueLine, voiceLine);
        }
    }

    private void Update()
    {
        //debug only - F key works on both QWERTY and AZERTY keyboards
        if (Input.GetKeyDown(KeyCode.F))
        {
            npcToTalkTo.Talk(dialogueLine, voiceLine);
            NPCManager.Instance.StartSpawning();
        }
    }
}