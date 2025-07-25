using UnityEngine;
using UnityEngine.UI;

public class ObjectiveManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The player's transform to track their position.")]
    public Transform playerTransform;

    [Tooltip("The PREFAB of the GameObject that will visualize the objective area (the force field cylinder).")]
    public GameObject objectiveAreaVisualizerPrefab;

    [Tooltip("The UI Image that acts as an arrow pointing to the objective.")]
    public Image directionIndicator;

    [Tooltip("The main camera used to translate world to screen coordinates.")]
    public Camera mainCamera;

    [Header("Objective State")]
    [Tooltip("The current active objective asset.")]
    public Objective currentObjective;
    
    [Header("Initial Objective")]
    [Tooltip("The first objective to load when the game starts.")]
    public Objective initialObjective;

    [Header("Visualizer Settings")]
    [Tooltip("The height of the cylindrical force field.")]
    public float visualizerHeight = 100f;

    [Header("Indicator Settings")]
    [Tooltip("How far from the screen edge the arrow will be.")]
    public float indicatorPadding = 75f;

    [Header("GUI Settings")]
    [Tooltip("Font size for the objective name.")]
    public int objectiveNameFontSize = 24;
    
    [Tooltip("Font size for the objective description.")]
    public int objectiveDescriptionFontSize = 16;
    
    [Tooltip("Font size for the progress counter.")]
    public int progressFontSize = 18;
    
    [Tooltip("Background color for the objective display.")]
    public Color guiBackgroundColor = new Color(0f, 0f, 0f, 0.7f);
    
    [Tooltip("Text color for the objective display.")]
    public Color guiTextColor = Color.white;

    [Tooltip("Text color for the progress counter.")]
    public Color progressTextColor = new Color(0f, 1f, 0f, 1f); // Green by default

    // --- Private instance variables ---
    private GameObject visualizerInstance;
    private Renderer visualizerRenderer;
    private GUIStyle objectiveNameStyle;
    private GUIStyle objectiveDescriptionStyle;
    private GUIStyle progressStyle;
    private GUIStyle backgroundStyle;

    void Start()
    {
        if (!playerTransform || !objectiveAreaVisualizerPrefab || !directionIndicator || !mainCamera)
        {
            Debug.LogError("Objective Manager is missing one or more references! Please assign them in the inspector.");
            enabled = false;
            return;
        }

        // --- FIX: Instantiate the prefab at the start ---
        visualizerInstance = Instantiate(objectiveAreaVisualizerPrefab);

        visualizerRenderer = visualizerInstance.GetComponent<Renderer>();
        if (visualizerRenderer == null)
        {
            Debug.LogError("The 'objectiveAreaVisualizer' PREFAB is missing a Renderer component!");
            enabled = false;
            // Clean up the instance we just created
            if (visualizerInstance) Destroy(visualizerInstance);
            return;
        }

        // Initially hide both visuals
        visualizerInstance.SetActive(false);
        directionIndicator.gameObject.SetActive(false);

        // Load initial objective if no current objective is set
        if (currentObjective == null && initialObjective != null)
        {
            currentObjective = initialObjective;
            Debug.Log($"Loaded initial objective: {currentObjective.objectiveName}");
        }

        if (currentObjective != null)
        {
            SetupObjectiveVisuals();
        }

        // Initialize GUI styles
        InitializeGUIStyles();
    }

    void InitializeGUIStyles()
    {
        // Create styles for GUI display
        objectiveNameStyle = new GUIStyle();
        objectiveNameStyle.fontSize = objectiveNameFontSize;
        objectiveNameStyle.normal.textColor = guiTextColor;
        objectiveNameStyle.fontStyle = FontStyle.Bold;
        objectiveNameStyle.alignment = TextAnchor.UpperLeft;
        objectiveNameStyle.wordWrap = true;

        objectiveDescriptionStyle = new GUIStyle();
        objectiveDescriptionStyle.fontSize = objectiveDescriptionFontSize;
        objectiveDescriptionStyle.normal.textColor = guiTextColor;
        objectiveDescriptionStyle.fontStyle = FontStyle.Normal;
        objectiveDescriptionStyle.alignment = TextAnchor.UpperLeft;
        objectiveDescriptionStyle.wordWrap = true;

        progressStyle = new GUIStyle();
        progressStyle.fontSize = progressFontSize;
        progressStyle.normal.textColor = progressTextColor;
        progressStyle.fontStyle = FontStyle.Bold;
        progressStyle.alignment = TextAnchor.UpperLeft;
        progressStyle.wordWrap = false;

        backgroundStyle = new GUIStyle();
        backgroundStyle.normal.background = CreateColorTexture(guiBackgroundColor);
    }

    private Texture2D CreateColorTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    void OnGUI()
    {
        if (currentObjective == null) return;

        // Calculate content dimensions
        float padding = 15f;
        float maxWidth = Screen.width * 0.4f; // Max 40% of screen width
        float lineSpacing = 5f;

        // Calculate text dimensions
        GUIContent nameContent = new GUIContent(currentObjective.objectiveName);
        GUIContent descContent = new GUIContent(currentObjective.description);

        float nameHeight = objectiveNameStyle.CalcHeight(nameContent, maxWidth);
        float descHeight = objectiveDescriptionStyle.CalcHeight(descContent, maxWidth);

        // Calculate progress height if needed
        float progressHeight = 0f;
        GUIContent progressContent = null;
        if (currentObjective.hasProgressCounter)
        {
            progressContent = new GUIContent("Progress: " + currentObjective.GetProgressText());
            progressHeight = progressStyle.CalcHeight(progressContent, maxWidth);
        }

        // Calculate total box dimensions
        float totalWidth = maxWidth + (padding * 2);
        float totalHeight = nameHeight + descHeight + lineSpacing + (padding * 2);
        
        // Add progress height if we have a counter
        if (currentObjective.hasProgressCounter)
        {
            totalHeight += progressHeight + lineSpacing;
        }

        // Draw background box
        Rect backgroundRect = new Rect(10, 10, totalWidth, totalHeight);
        GUI.Box(backgroundRect, "", backgroundStyle);

        float currentY = 10 + padding;

        // Draw objective name
        Rect nameRect = new Rect(10 + padding, currentY, maxWidth, nameHeight);
        GUI.Label(nameRect, nameContent, objectiveNameStyle);
        currentY += nameHeight + lineSpacing;

        // Draw objective description
        Rect descRect = new Rect(10 + padding, currentY, maxWidth, descHeight);
        GUI.Label(descRect, descContent, objectiveDescriptionStyle);
        currentY += descHeight;

        // Draw progress if applicable
        if (currentObjective.hasProgressCounter && progressContent != null)
        {
            currentY += lineSpacing;
            Rect progressRect = new Rect(10 + padding, currentY, maxWidth, progressHeight);
            GUI.Label(progressRect, progressContent, progressStyle);
        }
    }

    void Update()
    {
        if (currentObjective == null)
        {
            if (visualizerInstance.activeSelf) visualizerInstance.SetActive(false);
            if (directionIndicator.gameObject.activeSelf) directionIndicator.gameObject.SetActive(false);
            return;
        }

        // Hide direction indicator and area visualizer for the final "GG" objective
        if (currentObjective.objectiveName == "GG")
        {
            if (directionIndicator.gameObject.activeSelf) 
            {
                directionIndicator.gameObject.SetActive(false);
                Debug.Log("🏆 ObjectiveManager: Direction indicator hidden for final GG objective!");
            }
            if (visualizerInstance.activeSelf)
            {
                visualizerInstance.SetActive(false);
                Debug.Log("🏆 ObjectiveManager: Area visualizer hidden for final GG objective!");
            }
            return;
        }

        float distanceToObjective = Vector3.Distance(playerTransform.position, currentObjective.worldPosition);

        if (distanceToObjective <= currentObjective.areaRadius)
        {
            directionIndicator.gameObject.SetActive(false);
        }
        else
        {
            directionIndicator.gameObject.SetActive(true);
            UpdateDirectionIndicator();
        }
    }

    public void SetNewObjective(Objective newObjective)
    {
        currentObjective = newObjective;
        if (currentObjective != null)
        {
            // Ensure instance exists before setting up visuals
            if (visualizerInstance == null) return;
            SetupObjectiveVisuals();
        }
    }

    private void SetupObjectiveVisuals()
    {
        visualizerInstance.transform.position = currentObjective.worldPosition;

        visualizerInstance.transform.localScale = new Vector3(
            currentObjective.areaRadius * 2,
            visualizerHeight,
            currentObjective.areaRadius * 2
        );

        // FIX: We now modify the material of the INSTANCE, which is allowed.
        visualizerRenderer.material.SetColor("_Color", currentObjective.areaColor);
        visualizerRenderer.material.SetFloat("_FadeHeight", visualizerHeight);

        visualizerInstance.SetActive(true);
    }

    private void UpdateDirectionIndicator()
    {
        // Create a new target position on the same horizontal plane as the camera to ignore height differences.
        Vector3 flatTargetPosition = new Vector3(currentObjective.worldPosition.x, mainCamera.transform.position.y, currentObjective.worldPosition.z);

        Vector3 screenPos = mainCamera.WorldToScreenPoint(flatTargetPosition);
        Vector3 screenCenter = new Vector3(Screen.width, Screen.height, 0) / 2f;

        // --- Rotation Logic ---
        Vector3 directionToTarget = (flatTargetPosition - mainCamera.transform.position).normalized;
        float dotProduct = Vector3.Dot(mainCamera.transform.forward, directionToTarget);

        if (dotProduct < 0)
        {
            screenPos = -screenPos + 2 * screenCenter;
        }

        Vector3 dir = (screenPos - screenCenter).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        directionIndicator.rectTransform.rotation = Quaternion.Euler(0, 0, angle);

        // --- Position Clamping Logic ---
        float clampedX = Mathf.Clamp(screenPos.x, indicatorPadding, Screen.width - indicatorPadding);
        float clampedY = Mathf.Clamp(screenPos.y, indicatorPadding, Screen.height - indicatorPadding);

        if (clampedX == screenPos.x && clampedY == screenPos.y)
        {
            Vector3 fromCenter = screenPos - screenCenter;
            float angleRad = Mathf.Atan2(fromCenter.y, fromCenter.x);
            float cos = Mathf.Cos(angleRad);
            float sin = Mathf.Sin(angleRad);

            float m = sin / cos;
            float paddedWidth = Screen.width - indicatorPadding * 2;
            float paddedHeight = Screen.height - indicatorPadding * 2;

            if (cos > 0)
            {
                clampedX = screenCenter.x + paddedWidth / 2f;
            }
            else
            {
                clampedX = screenCenter.x - paddedWidth / 2f;
            }
            clampedY = screenCenter.y + m * (clampedX - screenCenter.x);

            if (clampedY > screenCenter.y + paddedHeight / 2f)
            {
                clampedY = screenCenter.y + paddedHeight / 2f;
                clampedX = screenCenter.x + (clampedY - screenCenter.y) / m;
            }
            else if (clampedY < screenCenter.y - paddedHeight / 2f)
            {
                clampedY = screenCenter.y - paddedHeight / 2f;
                clampedX = screenCenter.x + (clampedY - screenCenter.y) / m;
            }
        }

        directionIndicator.rectTransform.position = new Vector3(clampedX, clampedY, 0);
    }

    /// <summary>
    /// Updates the progress of the current objective
    /// </summary>
    /// <param name="incrementBy">Amount to increment progress by (default: 1)</param>
    public void UpdateObjectiveProgress(int incrementBy = 1)
    {
        if (currentObjective != null && currentObjective.hasProgressCounter)
        {
            for (int i = 0; i < incrementBy; i++)
            {
                currentObjective.IncrementProgress();
            }
            
            // Check if objective is complete
            if (currentObjective.IsComplete())
            {
                Debug.Log($"Objective '{currentObjective.objectiveName}' completed! ({currentObjective.GetProgressText()})");
                // You can add completion logic here (e.g., play sound, show notification, etc.)
            }
            else
            {
                Debug.Log($"Objective progress updated: {currentObjective.GetProgressText()}");
            }
        }
    }

    /// <summary>
    /// Sets the progress of the current objective to a specific value
    /// </summary>
    /// <param name="count">The progress count to set</param>
    public void SetObjectiveProgress(int count)
    {
        if (currentObjective != null && currentObjective.hasProgressCounter)
        {
            currentObjective.SetProgress(count);
            
            // Check if objective is complete
            if (currentObjective.IsComplete())
            {
                Debug.Log($"Objective '{currentObjective.objectiveName}' completed! ({currentObjective.GetProgressText()})");
            }
            else
            {
                Debug.Log($"Objective progress set to: {currentObjective.GetProgressText()}");
            }
        }
    }

    /// <summary>
    /// Gets the singleton instance of ObjectiveManager (if it exists)
    /// </summary>
    public static ObjectiveManager Instance { get; private set; }

    void Awake()
    {
        // Simple singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.LogWarning("Multiple ObjectiveManager instances found. Destroying duplicate.");
            Destroy(gameObject);
        }
    }
}