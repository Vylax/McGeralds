using UnityEngine;

/// <summary>
/// Generic interactable object that can complete objectives when player interacts with it.
/// Works with the existing PlayerInteraction system.
/// </summary>
public class InteractableObject : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("The name of this interactable object (shown in debug).")]
    public string objectName = "Interactable Object";
    
    [Header("Objective Completion")]
    [Tooltip("The objective name that should be completed when interacting with this object.")]
    public string objectiveToComplete;
    
    [Tooltip("The next objective to switch to after completion.")]
    public Objective nextObjective;
    
    [Header("Audio")]
    [Tooltip("Sound to play when interacting with this object.")]
    public AudioClip interactionSound;
    
    private AudioSource audioSource;
    private bool hasCompleted = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    /// <summary>
    /// Called when the player interacts with this object
    /// </summary>
    public void OnPlayerInteract()
    {
        if (hasCompleted)
        {
            Debug.Log($"Already interacted with {objectName}");
            return;
        }
        
        Debug.Log($"Player interacted with {objectName}");
        
        // Check if the current objective matches what we need to complete
        if (ObjectiveManager.Instance != null && ObjectiveManager.Instance.currentObjective != null)
        {
            if (ObjectiveManager.Instance.currentObjective.objectiveName == objectiveToComplete)
            {
                CompleteObjective();
            }
            else
            {
                Debug.Log($"Current objective '{ObjectiveManager.Instance.currentObjective.objectiveName}' doesn't match required '{objectiveToComplete}'");
            }
        }
        
        // Play interaction sound
        if (interactionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(interactionSound);
        }
    }
    
    private void CompleteObjective()
    {
        hasCompleted = true;
        
        Debug.Log($"Objective '{objectiveToComplete}' completed by interacting with {objectName}!");
        
        // Switch to next objective
        if (nextObjective != null && ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.SetNewObjective(nextObjective);
            Debug.Log($"Switched to next objective: {nextObjective.objectiveName}");
        }
    }
} 