using UnityEngine;

/// <summary>
/// Manager NPC that handles the "Talk to your manager" objective completion.
/// This script should be attached to the manager NPC GameObject along with NPCTrigger and DynamicSpeechBubble.
/// </summary>
public class ManagerNPC : MonoBehaviour
{
    [Header("Manager Settings")]
    [Tooltip("The boom sound to play when the objective is completed.")]
    public AudioClip boomSound;
    
    [Tooltip("The ice cream machine GameObject with MachineController to break when objective completes.")]
    public GameObject iceCreamMachine;
    
    [Tooltip("Machine piece GameObjects to enable after talking to manager.")]
    public GameObject[] machinePiecesToEnable;
    
    [Tooltip("The repair machine objective to switch to after completion.")]
    public Objective repairMachineObjective;
    
    private AudioSource audioSource;
    private bool hasCompletedObjective = false;

    void Start()
    {
        // Get required components
        audioSource = GetComponent<AudioSource>();
    }
    
    /// <summary>
    /// Call this method when the player talks to the manager
    /// </summary>
    public void OnPlayerTalkToManager()
    {
        if (!hasCompletedObjective)
        {
            CompleteManagerObjective();
        }
    }
    
    private void CompleteManagerObjective()
    {
        hasCompletedObjective = true;
        
        // Check if the current objective is "Talk to your manager"
        if (ObjectiveManager.Instance != null && ObjectiveManager.Instance.currentObjective != null)
        {
            if (ObjectiveManager.Instance.currentObjective.objectiveName == "Talk to your manager")
            {
                Debug.Log("Manager objective completed!");
                
                // Play boom sound
                if (boomSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(boomSound);
                }
                
                // Disable ice cream machine mesh renderer
                DisableIceCreamMachine();
                
                // Enable machine pieces
                EnableMachinePieces();
                
                // Switch to repair machine objective
                if (repairMachineObjective != null)
                {
                    ObjectiveManager.Instance.SetNewObjective(repairMachineObjective);
                    Debug.Log("Switched to repair machine objective");
                }
            }
        }
    }
    
    private void DisableIceCreamMachine()
    {
        Debug.Log("DisableIceCreamMachine() called");
        
        // If no machine assigned, try to find it automatically
        if (iceCreamMachine == null)
        {
            Debug.Log("Ice cream machine not assigned, searching for MachineController in scene...");
            MachineController foundController = FindObjectOfType<MachineController>();
            if (foundController != null)
            {
                iceCreamMachine = foundController.gameObject;
                Debug.Log($"Found ice cream machine automatically: {iceCreamMachine.name}");
            }
        }
        
        if (iceCreamMachine != null)
        {
            Debug.Log($"Using ice cream machine: {iceCreamMachine.name}");
            MachineController machineController = iceCreamMachine.GetComponent<MachineController>();
            if (machineController != null)
            {
                Debug.Log("Found MachineController, calling BreakMachine()");
                machineController.BreakMachine();
                Debug.Log($"Broke ice cream machine: {iceCreamMachine.name}");
            }
            else
            {
                Debug.LogWarning($"Ice cream machine {iceCreamMachine.name} doesn't have a MachineController component!");
            }
        }
        else
        {
            Debug.LogWarning("Ice cream machine is null and couldn't find MachineController in scene! Please assign it in the ManagerNPC component.");
        }
    }
    
    private void EnableMachinePieces()
    {
        if (machinePiecesToEnable == null) return;
        
        foreach (GameObject piece in machinePiecesToEnable)
        {
            if (piece != null)
            {
                piece.SetActive(true);
                Debug.Log($"Enabled machine piece: {piece.name}");
            }
        }
        
        Debug.Log($"Enabled {machinePiecesToEnable.Length} machine pieces");
    }
} 