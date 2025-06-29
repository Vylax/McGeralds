using UnityEngine;

public class MachineController : MonoBehaviour
{
    [Tooltip("The tag assigned to the collectible machine pieces.")]
    [SerializeField] private string collectibleTag = "MachinePiece";

    [Header("Objective Integration")]
    [Tooltip("Should this machine update the current objective progress?")]
    [SerializeField] private bool updateObjectiveProgress = true;
    
    [Tooltip("The next objective to switch to after machine repair is complete")]
    [SerializeField] private Objective nextObjective;

    private int _totalPieces;
    private int _piecesPlaced = 0;

    /// <summary>
    /// This method is called when the script instance is being loaded.
    /// </summary>
    void Start()
    {
        // Count the total number of pieces (but don't disable them yet - wait for manager interaction)
        _totalPieces = 13;
        
        // Initially disable any particle systems (they should only be active when machine is broken)
        DisableAllParticleSystems(transform);
    }
    
    /// <summary>
    /// Call this method to break the machine (disable all mesh renderers)
    /// This should be called when the manager objective is completed
    /// </summary>
    public void BreakMachine()
    {
        Debug.Log($"BreakMachine() called! Total pieces: {_totalPieces}");
        
        // Disable the main machine mesh renderer and all its children
        DisableAllMeshRenderers(transform);
        
        // Enable particle systems (smoke, sparks, etc. for broken machine effect)
        EnableAllParticleSystems(transform);
        
        Debug.Log("Machine broken! All mesh renderers disabled and particle systems enabled.");
    }

    /// <summary>
    /// This method is called when another collider enters the trigger.
    /// </summary>
    /// <param name="other">The other Collider involved in this collision.</param>
    private void OnTriggerEnter(Collider other)
    {
        // Safety check: Make sure the collider and its gameObject still exist
        if (other == null || other.gameObject == null)
        {
            return;
        }

        // Check if the entering object has the correct tag.
        if (other.CompareTag(collectibleTag))
        {
            // Find the corresponding child piece by name.
            Transform pieceToEnable = transform.Find(other.gameObject.name);

            if (pieceToEnable != null)
            {
                // Enable the mesh renderer of the corresponding piece and all its children
                EnableAllMeshRenderers(pieceToEnable);

                // Destroy the piece the player just placed.
                Destroy(other.gameObject);

                // Increment the counter for placed pieces.
                _piecesPlaced++;

                // Update objective progress if enabled
                if (updateObjectiveProgress && ObjectiveManager.Instance != null)
                {
                    ObjectiveManager.Instance.UpdateObjectiveProgress();
                }

                Debug.Log($"Machine piece placed! Progress: {_piecesPlaced}/{_totalPieces}");

                // Check if all pieces have been placed.
                if (_piecesPlaced >= _totalPieces)
                {
                    TaskDone();
                }
            }
            else
            {
                Debug.LogWarning("No corresponding piece found for: " + other.gameObject.name);
            }
        }
    }

    /// <summary>
    /// This method is called when the repair task is complete.
    /// </summary>
    private void TaskDone()
    {
        // Add your logic here for when the machine is fully repaired.
        Debug.Log("🎉 Machine repair complete! 🎉");
        
        // Disable particle systems (machine is no longer broken)
        DisableAllParticleSystems(transform);
        
        // Optional: Mark objective as complete and switch to next objective
        if (updateObjectiveProgress && ObjectiveManager.Instance != null)
        {
            // Ensure the objective progress is set to the target (in case of any discrepancies)
            ObjectiveManager.Instance.SetObjectiveProgress(_totalPieces);
            
            // Switch to the next objective if specified
            if (nextObjective != null)
            {
                Debug.Log($"Switching to next objective: {nextObjective.objectiveName}");
                ObjectiveManager.Instance.SetNewObjective(nextObjective);
            }
        }
    }

    /// <summary>
    /// Gets the current progress of the machine repair
    /// </summary>
    public float GetProgress()
    {
        return _totalPieces > 0 ? (float)_piecesPlaced / _totalPieces : 0f;
    }

    /// <summary>
    /// Gets the current progress as a formatted string
    /// </summary>
    public string GetProgressText()
    {
        return $"{_piecesPlaced}/{_totalPieces}";
    }
    
    /// <summary>
    /// Recursively disables all mesh renderers in a transform hierarchy
    /// </summary>
    private void DisableAllMeshRenderers(Transform parent)
    {
        // Disable mesh renderer on the parent if it exists
        MeshRenderer renderer = parent.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
            Debug.Log($"Disabled renderer on: {parent.name}");
        }
        
        // Recursively disable mesh renderers on all children
        for (int i = 0; i < parent.childCount; i++)
        {
            DisableAllMeshRenderers(parent.GetChild(i));
        }
    }
    
    /// <summary>
    /// Recursively enables all mesh renderers in a transform hierarchy
    /// </summary>
    private void EnableAllMeshRenderers(Transform parent)
    {
        // Enable mesh renderer on the parent if it exists
        MeshRenderer renderer = parent.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.enabled = true;
            Debug.Log($"Enabled renderer on: {parent.name}");
        }
        
        // Recursively enable mesh renderers on all children
        for (int i = 0; i < parent.childCount; i++)
        {
            EnableAllMeshRenderers(parent.GetChild(i));
        }
    }
    
    /// <summary>
    /// Recursively disables all particle systems in a transform hierarchy
    /// </summary>
    private void DisableAllParticleSystems(Transform parent)
    {
        // Disable particle system on the parent if it exists
        ParticleSystem particles = parent.GetComponent<ParticleSystem>();
        if (particles != null)
        {
            particles.gameObject.SetActive(false);
            Debug.Log($"Disabled particle system on: {parent.name}");
        }
        
        // Recursively disable particle systems on all children
        for (int i = 0; i < parent.childCount; i++)
        {
            DisableAllParticleSystems(parent.GetChild(i));
        }
    }
    
    /// <summary>
    /// Recursively enables all particle systems in a transform hierarchy
    /// </summary>
    private void EnableAllParticleSystems(Transform parent)
    {
        // Enable particle system on the parent if it exists
        ParticleSystem particles = parent.GetComponent<ParticleSystem>();
        if (particles != null)
        {
            particles.gameObject.SetActive(true);
            particles.Play();
            Debug.Log($"Enabled and started particle system on: {parent.name}");
        }
        
        // Recursively enable particle systems on all children
        for (int i = 0; i < parent.childCount; i++)
        {
            EnableAllParticleSystems(parent.GetChild(i));
        }
    }
}