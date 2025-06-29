// NPCManager.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance { get; private set; }

    [Header("Spawning Configuration")]
    [Tooltip("The NPC prefab to be spawned.")]
    public GameObject npcPrefab;
    [Tooltip("A list of points where NPCs can be spawned.")]
    public Transform[] spawnPoints;
    [Tooltip("The maximum number of NPCs allowed in the scene.")]
    public int maxNPCs = 10;
    [Tooltip("The time in seconds between each spawn attempt.")]
    public float spawnInterval = 30f;

    private List<GameObject> spawnedNPCs = new List<GameObject>();
    private List<Transform> availableSpawnPoints = new List<Transform>();
    private Coroutine spawnCoroutine;

    private void Awake()
    {
        // Singleton pattern implementation
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Keep across scenes
        }
        else
        {
            Debug.LogWarning("Multiple NPCManager instances detected. Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartSpawning();
    }

    // Call this method to begin the spawning process
    public void StartSpawning()
    {
        if (spawnCoroutine == null)
        {
            // Initialize available spawn points
            availableSpawnPoints.Clear();
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                availableSpawnPoints.Add(spawnPoints[i]);
            }
            
            Debug.Log("Starting NPC spawn cycle.");
            spawnCoroutine = StartCoroutine(SpawnCycle());
        }
        else
        {
            Debug.LogWarning("Spawn cycle is already running.");
        }
    }

    private IEnumerator SpawnCycle()
    {
        while (true)
        {
            // Clean up the list by removing any destroyed NPCs
            spawnedNPCs.RemoveAll(npc => npc == null);

            // Check if we can spawn more NPCs and have available spawn points
            if (spawnedNPCs.Count < maxNPCs && availableSpawnPoints.Count > 0)
            {
                SpawnNPC();
            }
            else if (availableSpawnPoints.Count == 0)
            {
                Debug.Log("No more available spawn points. Pausing spawn cycle.");
                break; // Exit the coroutine
            }
            else
            {
                Debug.Log("Maximum NPC limit reached. Pausing spawn cycle.");
                break; // Exit the coroutine
            }

            // Wait for the specified interval before the next spawn
            yield return new WaitForSeconds(spawnInterval);
        }
        spawnCoroutine = null; // Reset the coroutine reference
    }

    private void SpawnNPC()
    {
        if (npcPrefab == null || availableSpawnPoints.Count == 0)
        {
            Debug.LogError("NPC Prefab is not set or no available spawn points in the NPCManager.");
            return;
        }

        // Pick a random available spawn point
        int randomIndex = Random.Range(0, availableSpawnPoints.Count);
        Transform selectedSpawnPoint = availableSpawnPoints[randomIndex];

        // Remove the spawn point from available list so it won't be used again
        availableSpawnPoints.RemoveAt(randomIndex);

        // Instantiate the NPC and add it to our list
        GameObject newNPC = Instantiate(npcPrefab, selectedSpawnPoint.position, selectedSpawnPoint.rotation);
        
        spawnedNPCs.Add(newNPC);
        Debug.Log($"Spawned a new NPC at {selectedSpawnPoint.position}. Total count: {spawnedNPCs.Count}, Available spawn points: {availableSpawnPoints.Count}");
    }

    // Optional: A public method to stop spawning if needed
    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
            Debug.Log("NPC spawn cycle stopped.");
        }
    }

    // Call this method when an NPC is destroyed to free up its spawn point
    public void FreeSpawnPoint(Vector3 npcPosition)
    {
        // Find the closest spawn point to the NPC's position
        Transform closestSpawnPoint = null;
        float closestDistance = float.MaxValue;
        
        foreach (Transform spawnPoint in spawnPoints)
        {
            float distance = Vector3.Distance(npcPosition, spawnPoint.position);
            if (distance < closestDistance && !availableSpawnPoints.Contains(spawnPoint))
            {
                closestDistance = distance;
                closestSpawnPoint = spawnPoint;
            }
        }
        
        if (closestSpawnPoint != null)
        {
            availableSpawnPoints.Add(closestSpawnPoint);
            Debug.Log($"Freed spawn point at {closestSpawnPoint.position}. Available spawn points: {availableSpawnPoints.Count}");
        }
    }
}