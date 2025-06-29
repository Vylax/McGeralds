using UnityEngine;
using KinematicCharacterController.Examples;

public class PlayerRespawn : MonoBehaviour
{
    [Header("Respawn Settings")]
    [Tooltip("The Y position below which the player is considered to have fallen out of the map.")]
    public float fallThreshold = -10f;
    
    [Tooltip("The position where the player will respawn.")]
    public Vector3 respawnPosition = new Vector3(176.2f, 0.08f, 122.5f);
    
    private GameObject player;
    private bool isRespawning = false;

    void Start()
    {
        // Find the player GameObject by name and tag
        FindPlayer();
    }

    void Update()
    {
        // Check if player exists and has fallen below the threshold
        if (player != null && player.transform.position.y < fallThreshold && !isRespawning)
        {
            RespawnPlayer();
        }
        else if (player == null)
        {
            // Try to find the player again if it wasn't found initially
            FindPlayer();
        }
        else if (player != null && player.transform.position.y >= fallThreshold)
        {
            // Reset respawning flag when player is back on safe ground
            isRespawning = false;
        }
    }

    private void FindPlayer()
    {
        if (player != null) return;
        
        // Find player by tag
        player = GameObject.FindWithTag("Player");
        
        if (player != null)
        {
            Debug.Log($"Player found: {player.name}");
        }
        else
        {
            Debug.LogWarning("Player not found! Make sure there's a GameObject with tag 'Player'.");
        }
    }

    private void RespawnPlayer()
    {
        if (player == null || isRespawning) return;

        isRespawning = true;
        Debug.Log($"Player fell out of map! Teleporting from {player.transform.position} to {respawnPosition}");

        // Use ExampleCharacterController for proper teleportation
        ExampleCharacterController cc = player.GetComponent<ExampleCharacterController>();
        if (cc)
        {
            cc.Motor.SetPositionAndRotation(respawnPosition, Quaternion.identity);
            Debug.Log("Player teleported using ExampleCharacterController.Motor");
        }
        else
        {
            Debug.LogError("ExampleCharacterController not found on player! Cannot teleport properly.");
        }
    }

    // Optional: Draw the fall threshold in the scene view for debugging
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        // Draw a plane at the fall threshold
        Vector3 center = transform.position;
        center.y = fallThreshold;
        Gizmos.DrawWireCube(center, new Vector3(100f, 0.1f, 100f));
        
        // Draw the respawn position
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(respawnPosition, 1f);
        Gizmos.DrawRay(respawnPosition, Vector3.up * 3f);
    }
} 