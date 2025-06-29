using UnityEngine;

public class BillboardSprite : MonoBehaviour
{
    public bool lockVerticalRotation = false; // Public boolean to control vertical rotation lock

    private Transform mainCameraTransform;

    void Start()
    {
        // Find the main camera in the scene. It's good practice to tag your main camera as "MainCamera".
        mainCameraTransform = Camera.main.transform;

        if (mainCameraTransform == null)
        {
            Debug.LogError("BillboardSprite: No main camera found! Please ensure your camera is tagged as 'MainCamera'.");
            enabled = false; // Disable the script if no camera is found.
        }
    }

    void LateUpdate()
    {
        if (mainCameraTransform == null)
        {
            return; // Don't do anything if no camera is assigned.
        }

        // Calculate the direction from the sprite to the camera
        Vector3 lookDirection = mainCameraTransform.position - transform.position;

        if (lockVerticalRotation)
        {
            // If vertical rotation is locked, flatten the look direction
            // by setting the Y component to 0. This makes the sprite only rotate
            // around its own Y-axis to face the camera horizontally.
            lookDirection.y = 0;
        }

        // Ensure there's a valid direction to look at
        if (lookDirection != Vector3.zero)
        {
            // Create a rotation that looks along the 'lookDirection'
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

            // Apply the rotation to the sprite
            transform.rotation = targetRotation;
        }
    }
}