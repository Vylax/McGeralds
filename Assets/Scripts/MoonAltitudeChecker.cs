using UnityEngine;

/// <summary>
/// Monitors player altitude during the moon objective.
/// Switches to "GG" objective when player reaches Y > 30.
/// </summary>
public class MoonAltitudeChecker : MonoBehaviour
{
    [Header("Altitude Settings")]
    [Tooltip("The Y position threshold to trigger the GG objective.")]
    public float altitudeThreshold = 30f;
    
    [Tooltip("The objective name that should be active for altitude checking.")]
    public string moonObjectiveName = "Go to the Moon";
    
    [Tooltip("The objective to switch to when altitude is reached.")]
    public Objective congratsObjective;
    
    [Header("Player Detection")]
    [Tooltip("The player GameObject. Leave empty to auto-find by tag 'Player'.")]
    public GameObject player;
    
    [Tooltip("Tag to search for if player is not manually assigned.")]
    public string playerTag = "Player";
    
    [Header("Debug")]
    [Tooltip("Show debug messages in console.")]
    public bool debugMode = true;
    
    private bool objectiveCompleted = false;
    private bool wasCheckingAltitude = false;

    void Start()
    {
        // Auto-find player if not assigned
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag(playerTag);
            if (player == null)
            {
                // Try alternative methods
                player = FindObjectOfType<KinematicCharacterController.Examples.ExampleCharacterController>()?.gameObject;
            }
            
            if (debugMode)
            {
                if (player != null)
                    Debug.Log($"MoonAltitudeChecker: Auto-found player: {player.name}");
                else
                    Debug.LogWarning("MoonAltitudeChecker: Could not find player GameObject!");
            }
        }
    }

    void Update()
    {
        // Skip if objective already completed or no player found
        if (objectiveCompleted || player == null) return;
        
        // Check if we're currently on the moon objective
        bool shouldCheckAltitude = ShouldCheckAltitude();
        
        // Log when we start/stop checking altitude
        if (shouldCheckAltitude != wasCheckingAltitude)
        {
            wasCheckingAltitude = shouldCheckAltitude;
            if (debugMode)
            {
                if (shouldCheckAltitude)
                    Debug.Log($"🚀 MoonAltitudeChecker: Started monitoring altitude. Threshold: Y > {altitudeThreshold}");
                else
                    Debug.Log("🚀 MoonAltitudeChecker: Stopped monitoring altitude.");
            }
        }
        
        // Monitor altitude if we should be checking
        if (shouldCheckAltitude)
        {
            float currentAltitude = player.transform.position.y;
            
            if (debugMode && Time.frameCount % 60 == 0) // Log every ~1 second
            {
                Debug.Log($"🌙 Player altitude: {currentAltitude:F1} (threshold: {altitudeThreshold})");
            }
            
            if (currentAltitude > altitudeThreshold)
            {
                CompleteAltitudeObjective();
            }
        }
    }
    
    /// <summary>
    /// Checks if we should be monitoring player altitude
    /// </summary>
    private bool ShouldCheckAltitude()
    {
        return ObjectiveManager.Instance != null && 
               ObjectiveManager.Instance.currentObjective != null && 
               ObjectiveManager.Instance.currentObjective.objectiveName == moonObjectiveName;
    }
    
    /// <summary>
    /// Completes the altitude objective and switches to GG
    /// </summary>
    private void CompleteAltitudeObjective()
    {
        if (objectiveCompleted) return;
        
        objectiveCompleted = true;
        
        if (debugMode)
            Debug.Log($"🎉 MoonAltitudeChecker: Player reached the moon! Altitude: {player.transform.position.y:F1}");
        
        // Switch to congratulations objective
        if (congratsObjective != null && ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.SetNewObjective(congratsObjective);
            
            if (debugMode)
                Debug.Log($"🏆 Switched to final objective: {congratsObjective.objectiveName} - All objective indicators will now be hidden!");
        }
        else
        {
            Debug.LogWarning("MoonAltitudeChecker: No congratulations objective assigned!");
        }
    }
    
    /// <summary>
    /// Reset the altitude checker (for testing)
    /// </summary>
    [ContextMenu("Reset Altitude Checker")]
    public void ResetChecker()
    {
        objectiveCompleted = false;
        wasCheckingAltitude = false;
        if (debugMode)
            Debug.Log("MoonAltitudeChecker: Reset for testing");
    }
} 