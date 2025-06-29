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
}