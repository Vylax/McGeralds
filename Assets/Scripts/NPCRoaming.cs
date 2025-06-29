// NPCTalking.cs
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(DynamicSpeechBubble))]
public class NPCRoaming : MonoBehaviour
{
    [Header("Speech Settings")]
    [Tooltip("The voice clip to use for all dialogue.")]
    public AudioClip voiceClip;

    [Header("Talk Settings")]
    [Tooltip("Minimum time to wait before talking again.")]
    public float minTalkInterval = 3f;
    [Tooltip("Maximum time to wait before talking again.")]
    public float maxTalkInterval = 8f;
    [Range(0, 1)]
    [Tooltip("The probability (0 to 1) that the NPC will talk when the interval triggers.")]
    public float chanceToTalk = 0.8f;

    // Preset McGerald's ice cream queue dialogue
    private string[] dialogueLines = {
        "Why is the ice cream machine ALWAYS broken?!",
        "I just want an ice cream, is that too much to ask?",
        "Excuse me, is your ice cream machine working today?",
        "I've been to 3 McGerald's and none of them have working ice cream!",
        "Can I get a vanilla cone? Oh wait, let me guess... machine's broken?",
        "I specifically came here for ice cream and now you tell me it's broken?",
        "How hard is it to fix an ice cream machine? Seriously!",
        "I'm never coming back here again! ...until tomorrow for ice cream.",
        "Do you guys even TRY to fix the machine or just leave it broken?",
        "I bet if I worked here I could fix that machine in 5 minutes!",
        "My kid is crying because they want ice cream and your machine is broken AGAIN!",
        "Is there a conspiracy against McGerald's ice cream or something?",
        "I drove 20 minutes just for an ice cream and you're telling me no?",
        "Can't you just go to the store and buy some ice cream to sell?",
        "I'm calling corporate about this broken machine situation!",
        "Why don't you put a sign outside saying 'Ice Cream Machine Broken'?",
        "I'll take anything cold... a frozen burger, I don't care anymore!",
        "Is the machine actually broken or are you just too lazy to clean it?",
        "I bet the ice cream machine at Burger Emperor works!",
        "Can I speak to the manager about this ice cream situation?",
        "I just want to know WHY it's always broken!",
        "Do you have any ice cream in the back freezer I could buy?",
        "I'm starting to think McGerald's ice cream is just a myth!",
        "Next time I'm bringing my own ice cream to eat here!",
        "How am I supposed to enjoy my fries without a McGerald's ice cream?!"
    };

    private DynamicSpeechBubble speechBubble;

    void Start()
    {
        speechBubble = GetComponent<DynamicSpeechBubble>();

        // Start the random talking behavior
        StartCoroutine(RandomTalking());
    }

    private IEnumerator RandomTalking()
    {
        while (true)
        {
            // Wait for a random talk interval
            float talkInterval = Random.Range(minTalkInterval, maxTalkInterval);
            yield return new WaitForSeconds(talkInterval);

            // Check if the NPC should talk
            if (Random.value < chanceToTalk)
            {
                TryToTalk();
            }
        }
    }

    private void TryToTalk()
    {
        // Ensure the speech bubble component exists
        if (speechBubble != null && dialogueLines.Length > 0)
        {
            // Pick a random ice cream complaint
            int randomIndex = Random.Range(0, dialogueLines.Length);
            string dialogue = dialogueLines[randomIndex];
            
            Debug.Log($"NPC {gameObject.name} is talking: {dialogue}");
            
            // Call the talk function with the voice clip (can be null)
            speechBubble.Talk(dialogue, voiceClip);
        }
        else if (speechBubble == null)
        {
            Debug.LogError("DynamicSpeechBubble component not found on this GameObject.");
        }
    }
}