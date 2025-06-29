// This script must be placed in a folder named "Editor" in your Unity project's Assets directory.
// If you don't have an "Editor" folder, please create one.

using UnityEngine;
using UnityEditor;

/// <summary>
/// This class adds a custom menu item to the Unity Editor under "Tools > Add Mesh Colliders to Children".
/// It allows the user to add MeshColliders to a selected GameObject and all its descendants
/// that have a MeshFilter component but no MeshCollider component yet.
/// </summary>
public class AddMeshCollidersEditor
{
    // Defines the menu path for the new editor tool.
    private const string MenuItemPath = "Tools/Add Mesh Colliders to Children";

    /// <summary>
    /// This is the main function that gets executed when the user clicks the menu item.
    /// It finds all GameObjects with a MeshFilter in the selected hierarchy and adds a MeshCollider if one isn't present.
    /// </summary>
    [MenuItem(MenuItemPath)]
    private static void AddCollidersToSelectedObjectHierarchy()
    {
        // Get the currently active/selected GameObject from the Hierarchy window.
        GameObject selectedObject = Selection.activeGameObject;

        // This check is a safeguard. The menu item should be disabled if no object is selected,
        // but this ensures no errors occur if the function is somehow called without a selection.
        if (selectedObject == null)
        {
            EditorUtility.DisplayDialog("No Object Selected", "Please select a GameObject in the Hierarchy.", "OK");
            return;
        }

        // The Undo class allows this entire operation to be undone with a single Ctrl+Z / Cmd+Z.
        // The string "Add Mesh Colliders" will appear in the Edit > Undo menu.
        Undo.RegisterCompleteObjectUndo(selectedObject, "Add Mesh Colliders");

        int collidersAddedCount = 0;

        // GetComponentsInChildren<T>(true) retrieves the component from the parent object
        // and all of its children, even inactive ones.
        // We get all Transform components to iterate through every GameObject in the hierarchy.
        Transform[] allTransforms = selectedObject.GetComponentsInChildren<Transform>(true);

        foreach (Transform childTransform in allTransforms)
        {
            // Check if the current GameObject has a MeshFilter component.
            if (childTransform.GetComponent<MeshFilter>() != null)
            {
                // If it has a MeshFilter, check if it already has a MeshCollider.
                if (childTransform.GetComponent<MeshCollider>() == null)
                {
                    // If no MeshCollider is found, add one.
                    // We record this action with the Undo system as well.
                    var newCollider = childTransform.gameObject.AddComponent<MeshCollider>();
                    Undo.RegisterCreatedObjectUndo(newCollider, "Add Mesh Collider");
                    collidersAddedCount++;
                }
            }
        }

        // Provide feedback to the user via the console and a dialog box.
        if (collidersAddedCount > 0)
        {
            Debug.Log($"Successfully added {collidersAddedCount} MeshCollider(s) to the hierarchy of '{selectedObject.name}'.");
            EditorUtility.DisplayDialog("Success", $"Operation complete. Added {collidersAddedCount} new MeshCollider(s).", "OK");
        }
        else
        {
            Debug.Log($"No new MeshColliders were needed for the hierarchy of '{selectedObject.name}'. All meshes already had colliders.");
            EditorUtility.DisplayDialog("Operation Complete", "No new colliders were needed. All objects with a MeshFilter in the selection already had a MeshCollider.", "OK");
        }
    }

    /// <summary>
    /// This is a validation function for the menu item.
    /// Unity automatically calls this method before displaying the menu item.
    /// The return value determines if the menu item should be enabled or disabled.
    /// </summary>
    /// <returns>True if a GameObject is selected, false otherwise.</returns>
    [MenuItem(MenuItemPath, true)]
    private static bool ValidateAddColliders()
    {
        // The menu item will only be clickable if the user has selected a GameObject in the editor.
        return Selection.activeGameObject != null;
    }
}
