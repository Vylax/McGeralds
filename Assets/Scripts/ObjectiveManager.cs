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

    [Header("Visualizer Settings")]
    [Tooltip("The height of the cylindrical force field.")]
    public float visualizerHeight = 100f;

    [Header("Indicator Settings")]
    [Tooltip("How far from the screen edge the arrow will be.")]
    public float indicatorPadding = 75f;

    // --- Private instance variables ---
    private GameObject visualizerInstance;
    private Renderer visualizerRenderer;

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

        if (currentObjective != null)
        {
            SetupObjectiveVisuals();
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
}