using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// A self-contained component that generates a dynamic speech bubble above an NPC.
/// This script creates and manages all the necessary UI elements programmatically.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class DynamicSpeechBubble : MonoBehaviour
{
    // --- Public fields for customization in the Inspector ---

    [Header("Bubble Look & Feel")]
    [Tooltip("The sprite for the bubble's background. A 9-sliced sprite is recommended.")]
    public Sprite bubbleSprite;

    [Tooltip("The padding between the text and the edge of the bubble background.")]
    public Vector2 padding = new Vector2(30, 20);

    [Tooltip("The maximum width the text can have before it wraps to the next line.")]
    public float maxWidth = 400f;


    [Header("Text Style")]
    [Tooltip("The TMP_FontAsset to use. If left null, it will try to find a default one.")]
    public TMP_FontAsset fontAsset;
    public float fontSize = 36f;
    public Color textColor = Color.black;


    [Header("Positioning & Scale")]
    [Tooltip("The offset from the NPC's pivot point where the bubble will appear.")]
    public Vector3 bubbleOffset = new Vector3(0, 2f, 0);

    [Tooltip("The overall scale of the speech bubble in the world. Adjust this if the bubble is too big or too small.")]
    public float bubbleScale = 0.01f;


    // --- Private fields ---
    private AudioSource audioSource;
    private GameObject currentBubbleInstance;

    /// <summary>
    /// A simple component that makes the GameObject it's attached to always face the main camera.
    /// </summary>
    private class Billboard : MonoBehaviour
    {
        private Transform mainCameraTransform;

        void Awake()
        {
            // Find the main camera and store its transform for efficiency.
            if (Camera.main != null)
            {
                mainCameraTransform = Camera.main.transform;
            }
        }

        // Use LateUpdate to ensure the rotation is set after all other physics and updates.
        void LateUpdate()
        {
            if (mainCameraTransform != null)
            {
                // Set the bubble's rotation to exactly match the camera's rotation.
                // This makes it always appear flat towards the player.
                transform.rotation = mainCameraTransform.rotation;
            }
        }
    }


    /// <summary>
    /// Standard Unity Awake function. Initializes the component.
    /// </summary>
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Makes the NPC say something. This is the main function to call from other scripts.
    /// </summary>
    /// <param name="lineOfText">The text you want the NPC to say.</param>
    /// <param name="voiceClip">The audio clip to play while the bubble is visible.</param>
    public void Talk(string lineOfText, AudioClip voiceClip)
    {
        // If the NPC is already talking, stop the old coroutine and destroy the old bubble.
        if (currentBubbleInstance != null)
        {
            StopAllCoroutines();
            Destroy(currentBubbleInstance);
        }

        // Start a coroutine that creates the bubble, waits, and then destroys it.
        StartCoroutine(CreateAndDestroyBubble(lineOfText, voiceClip));
    }


    /// <summary>
    /// The main coroutine that handles the entire lifecycle of the speech bubble.
    /// </summary>
    private IEnumerator CreateAndDestroyBubble(string lineOfText, AudioClip voiceClip)
    {
        // 1. --- Create the Hierarchy ---
        // Create the main bubble GameObject that will hold all the UI elements.
        currentBubbleInstance = new GameObject("SpeechBubble");
        currentBubbleInstance.transform.SetParent(this.transform, false);
        currentBubbleInstance.transform.localPosition = bubbleOffset;

        currentBubbleInstance.transform.localScale = Vector3.one * bubbleScale;

        // Add a Canvas to make it a UI element that can render in the world.
        Canvas canvas = currentBubbleInstance.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;

        // ** FIX: Add the Billboard component to make it always face the camera every frame. **
        currentBubbleInstance.AddComponent<Billboard>();

        // Create the Text object first, as its size will determine the bubble's size.
        GameObject textObject = new GameObject("BubbleText");
        textObject.transform.SetParent(currentBubbleInstance.transform, false);
        TextMeshProUGUI textMesh = textObject.AddComponent<TextMeshProUGUI>();

        // 2. --- Configure the Text ---
        textMesh.font = fontAsset; // Assign font (can be null for default)
        textMesh.fontSize = fontSize;
        textMesh.color = textColor;
        textMesh.text = lineOfText;
        textMesh.alignment = TextAlignmentOptions.Center;

        // 3. --- Calculate the Perfect Size ---
        // This is the most important part. We ask TextMeshPro how big the text *wants* to be.
        Vector2 preferredSize = textMesh.GetPreferredValues(lineOfText, maxWidth, 0);
        // We set the text box size directly. The text will now wrap correctly.
        textMesh.rectTransform.sizeDelta = new Vector2(maxWidth, preferredSize.y);


        // 4. --- Create and Size the Bubble Background ---
        GameObject bubbleObject = new GameObject("BubbleBackground");
        bubbleObject.transform.SetParent(currentBubbleInstance.transform, false);
        // Move the background behind the text.
        bubbleObject.transform.SetAsFirstSibling();

        RectTransform bubbleRect = bubbleObject.AddComponent<RectTransform>();
        UnityEngine.UI.Image bubbleImage = bubbleObject.AddComponent<UnityEngine.UI.Image>();

        bubbleImage.sprite = bubbleSprite;
        bubbleImage.type = UnityEngine.UI.Image.Type.Sliced; // Use 9-slicing for clean resizing

        // Set the bubble's size to be the text's size plus the padding.
        Vector2 bubbleSize = new Vector2(preferredSize.x + padding.x, preferredSize.y + padding.y);
        bubbleRect.sizeDelta = bubbleSize;

        // 5. --- Set the Pivot for Upward Scaling ---
        // By setting the pivot's Y value to 0, the bubble will anchor at its bottom edge.
        // When its height increases, it will only grow upwards, away from the NPC's head.
        bubbleRect.pivot = new Vector2(0.5f, 0f);
        textMesh.rectTransform.pivot = new Vector2(0.5f, 0f);


        // 6. --- Play Audio and Wait ---
        if (voiceClip != null)
        {
            audioSource.PlayOneShot(voiceClip);
            // Wait for the duration of the audio clip before destroying the bubble.
            yield return new WaitForSeconds(voiceClip.length);
        }
        else
        {
            // If no clip is provided, wait for a default duration (e.g., 3 seconds).
            yield return new WaitForSeconds(3f);
        }

        // 7. --- Cleanup ---
        Destroy(currentBubbleInstance);
        currentBubbleInstance = null; // Clear the reference
    }
}
