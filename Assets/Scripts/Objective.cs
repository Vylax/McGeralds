// File: Objective.cs
// This script defines the data for a single objective.
// By using a ScriptableObject, you can create and save different objectives
// as assets in your project, which is great for organization.

using UnityEngine;

[CreateAssetMenu(fileName = "New Objective", menuName = "Game/Objective")]
public class Objective : ScriptableObject
{
    [Header("Objective Details")]
    public string objectiveName = "New Objective";
    [TextArea]
    public string description = "A short description of the objective.";

    [Header("Location & Area")]
    public Vector3 worldPosition;
    public float areaRadius = 20f;

    [Header("Visuals")]
    public Color areaColor = new Color(0f, 0.8f, 1f, 0.5f); // Default: semi-transparent blue

    [Header("Progress Tracking")]
    [Tooltip("Enable this for objectives that require collecting/completing multiple items")]
    public bool hasProgressCounter = false;
    [Tooltip("Target number of items/tasks to complete (only used if hasProgressCounter is true)")]
    public int targetCount = 1;
    [Tooltip("Current progress (managed at runtime, don't set manually)")]
    [System.NonSerialized] public int currentCount = 0;

    /// <summary>
    /// Increments the current progress count
    /// </summary>
    public void IncrementProgress()
    {
        if (hasProgressCounter)
        {
            currentCount = Mathf.Min(currentCount + 1, targetCount);
        }
    }

    /// <summary>
    /// Sets the current progress count
    /// </summary>
    public void SetProgress(int count)
    {
        if (hasProgressCounter)
        {
            currentCount = Mathf.Clamp(count, 0, targetCount);
        }
    }

    /// <summary>
    /// Returns true if the objective is complete
    /// </summary>
    public bool IsComplete()
    {
        if (hasProgressCounter)
        {
            return currentCount >= targetCount;
        }
        return false; // Non-counter objectives need manual completion logic
    }

    /// <summary>
    /// Gets the progress as a formatted string
    /// </summary>
    public string GetProgressText()
    {
        if (hasProgressCounter)
        {
            return $"{currentCount}/{targetCount}";
        }
        return "";
    }
}