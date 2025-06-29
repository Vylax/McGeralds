using UnityEngine;

public class MachineController : MonoBehaviour
{
    [Tooltip("The tag assigned to the collectible machine pieces.")]
    [SerializeField] private string collectibleTag = "MachinePiece";

    [Header("Objective Integration")]
    [Tooltip("Should this machine update the current objective progress?")]
    [SerializeField] private bool updateObjectiveProgress = true;

    private int _totalPieces;
    private int _piecesPlaced = 0;

    /// <summary>
    /// This method is called when the script instance is being loaded.
    /// </summary>
    void Start()
    {
        // Disable all child mesh renderers at the start and count the total number of pieces.
        _totalPieces = transform.childCount;
        for (int i = 0; i < _totalPieces; i++)
        {
            MeshRenderer pieceRenderer = transform.GetChild(i).GetComponent<MeshRenderer>();
            if (pieceRenderer != null)
            {
                pieceRenderer.enabled = false;
            }
        }
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
                // Enable the mesh renderer of the corresponding piece on the machine.
                MeshRenderer pieceRenderer = pieceToEnable.GetComponent<MeshRenderer>();
                if (pieceRenderer != null)
                {
                    pieceRenderer.enabled = true;
                }

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
        // For example, you could play an animation, show a UI message, or start the next quest.
        
        // Optional: Mark objective as complete if using ObjectiveManager
        if (updateObjectiveProgress && ObjectiveManager.Instance != null)
        {
            // Ensure the objective progress is set to the target (in case of any discrepancies)
            ObjectiveManager.Instance.SetObjectiveProgress(_totalPieces);
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
}