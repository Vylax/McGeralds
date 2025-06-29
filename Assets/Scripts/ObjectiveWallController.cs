using UnityEngine;

/// <summary>
/// Controls a hidden wall object based on objective changes.
/// Disables the wall when the "Go to the Moon" objective becomes active.
/// </summary>
public class ObjectiveWallController : MonoBehaviour
{
    [Header("Wall Settings")]
    [Tooltip("The wall GameObject to disable when the objective starts.")]
    public GameObject hiddenWall;
    
    [Tooltip("The objective name that should trigger the wall to be disabled.")]
    public string targetObjectiveName = "Go to the Moon";
    
    [Header("Debug")]
    [Tooltip("Show debug messages in console.")]
    public bool debugMode = true;
    
    private string lastObjectiveName = "";
    private bool wallDisabled = false;

    void Start()
    {
        // If no wall is assigned, try to find it on this GameObject
        if (hiddenWall == null)
        {
            hiddenWall = gameObject;
            if (debugMode)
                Debug.Log($"ObjectiveWallController: Using this GameObject as the hidden wall: {gameObject.name}");
        }
        
        // Make sure the wall starts enabled
        if (hiddenWall != null)
        {
            hiddenWall.SetActive(true);
            if (debugMode)
                Debug.Log($"ObjectiveWallController: Wall '{hiddenWall.name}' is initially enabled");
        }
    }

    void Update()
    {
        // Check if ObjectiveManager exists and has a current objective
        if (ObjectiveManager.Instance != null && ObjectiveManager.Instance.currentObjective != null)
        {
            string currentObjectiveName = ObjectiveManager.Instance.currentObjective.objectiveName;
            
            // Check if the objective has changed
            if (currentObjectiveName != lastObjectiveName)
            {
                lastObjectiveName = currentObjectiveName;
                
                if (debugMode)
                    Debug.Log($"ObjectiveWallController: Objective changed to '{currentObjectiveName}'");
                
                // Check if this is the target objective
                if (currentObjectiveName == targetObjectiveName && !wallDisabled)
                {
                    DisableWall();
                }
            }
        }
    }
    
    /// <summary>
    /// Disables the hidden wall
    /// </summary>
    public void DisableWall()
    {
        if (hiddenWall != null && !wallDisabled)
        {
            hiddenWall.SetActive(false);
            wallDisabled = true;
            
            if (debugMode)
                Debug.Log($"🚧 ObjectiveWallController: Hidden wall '{hiddenWall.name}' has been disabled! Path to the moon is now open! 🚀");
        }
    }
    
    /// <summary>
    /// Re-enables the hidden wall (for testing purposes)
    /// </summary>
    public void EnableWall()
    {
        if (hiddenWall != null)
        {
            hiddenWall.SetActive(true);
            wallDisabled = false;
            
            if (debugMode)
                Debug.Log($"🚧 ObjectiveWallController: Hidden wall '{hiddenWall.name}' has been re-enabled!");
        }
    }
    
    /// <summary>
    /// Manually trigger the wall disable (for testing)
    /// </summary>
    [ContextMenu("Disable Wall")]
    public void TestDisableWall()
    {
        DisableWall();
    }
    
    /// <summary>
    /// Manually trigger the wall enable (for testing)
    /// </summary>
    [ContextMenu("Enable Wall")]
    public void TestEnableWall()
    {
        EnableWall();
    }
} 