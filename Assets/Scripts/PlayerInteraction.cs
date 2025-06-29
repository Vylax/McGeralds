using UnityEngine;
using UnityEngine.UI;
using KinematicCharacterController.Examples;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 5f;
    [SerializeField] private float holdDistance = 2.5f;

    [Header("Physics Settings")]
    [SerializeField] private float springForce = 200f;
    [SerializeField] private float damper = 0.1f;
    [SerializeField] private float throwForce = 15f;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 5f;

    [Header("UI Settings")]
    [SerializeField] private Image interactionPrompt;

    private Rigidbody heldObject;
    private Camera mainCamera;
    private bool isRotatingObject = false;
    
    // References to camera and character controllers
    private ExampleCharacterCamera cameraController;
    private ExampleCharacterController characterController;

    void Start()
    {
        // Use Camera.main to find the primary camera in the scene
        mainCamera = Camera.main;
        if (interactionPrompt != null)
        {
            interactionPrompt.gameObject.SetActive(false);
        }

        // Find the camera controller component
        if (mainCamera != null)
        {
            cameraController = mainCamera.GetComponent<ExampleCharacterCamera>();
            if (cameraController == null)
            {
                // Try to find it in parent or child objects
                cameraController = mainCamera.GetComponentInParent<ExampleCharacterCamera>();
                if (cameraController == null)
                {
                    cameraController = mainCamera.GetComponentInChildren<ExampleCharacterCamera>();
                }
            }
        }

        // Find the character controller component (try this gameObject first, then parent)
        characterController = GetComponent<ExampleCharacterController>();
        if (characterController == null)
        {
            characterController = GetComponentInParent<ExampleCharacterController>();
        }

        // Log warnings if components are not found
        if (cameraController == null)
        {
            Debug.LogWarning("ExampleCharacterCamera component not found. Camera rotation locking will not work.");
        }
        if (characterController == null)
        {
            Debug.LogWarning("ExampleCharacterController component not found. Character rotation locking will not work.");
        }
    }

    void Update()
    {
        // --- 1. IF WE ARE HOLDING AN OBJECT ---
        if (heldObject != null)
        {
            HandleHeldObjectInputs();
        }
        // --- 2. IF WE ARE NOT HOLDING AN OBJECT ---
        else
        {
            CheckForGrabbableObject();
        }
    }

    void FixedUpdate()
    {
        if (heldObject != null)
        {
            MoveHeldObject();
            if (isRotatingObject)
            {
                RotateHeldObject();
            }
        }
    }

    /// <summary>
    /// Handles all player input when an object is currently being held.
    /// </summary>
    private void HandleHeldObjectInputs()
    {
        // Press E to drop
        if (Input.GetKeyDown(KeyCode.E))
        {
            DropObject();
            return;
        }

        // Left-click to throw
        if (Input.GetMouseButtonDown(0))
        {
            ThrowObject();
            return;
        }

        // Right-click to start rotating
        if (Input.GetMouseButtonDown(1))
        {
            isRotatingObject = true;
            ToggleCameraRotation(false); // Disable camera look
        }
        // Right-click release to stop rotating
        if (Input.GetMouseButtonUp(1))
        {
            isRotatingObject = false;
            ToggleCameraRotation(true); // Re-enable camera look
        }
    }

    /// <summary>
    /// Checks if the player is looking at a grabbable object and handles grabbing.
    /// </summary>
    private void CheckForGrabbableObject()
    {
        RaycastHit hit;
        bool isLookingAtGrabbable = Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out hit, interactionDistance)
                                     && hit.collider.attachedRigidbody != null
                                     && !hit.collider.CompareTag("Player");

        if (interactionPrompt != null)
        {
            interactionPrompt.gameObject.SetActive(isLookingAtGrabbable);
        }

        if (isLookingAtGrabbable && Input.GetKeyDown(KeyCode.E))
        {
            TryGrabObject(hit);
        }
    }

    private void TryGrabObject(RaycastHit hit)
    {
        heldObject = hit.collider.attachedRigidbody;
        heldObject.useGravity = false;
        heldObject.linearDamping = 10f;
        heldObject.angularDamping = 5f;
    }

    private void DropObject()
    {
        if (heldObject == null) return;

        // If we were rotating, make sure to re-enable the camera
        if (isRotatingObject)
        {
            isRotatingObject = false;
            ToggleCameraRotation(true);
        }

        heldObject.useGravity = true;
        heldObject.linearDamping = 0f;
        heldObject.angularDamping = 0.05f;
        heldObject = null;
    }

    private void ThrowObject()
    {
        if (heldObject == null) return;

        Rigidbody thrownBody = heldObject;
        DropObject(); // Release the object first

        // Apply the forward throw force
        thrownBody.AddForce(mainCamera.transform.forward * throwForce, ForceMode.Impulse);
    }


    private void MoveHeldObject()
    {
        Vector3 targetPosition = mainCamera.transform.position + mainCamera.transform.forward * holdDistance;
        Vector3 forceDirection = targetPosition - heldObject.position;
        heldObject.AddForce(forceDirection * springForce);
        heldObject.AddForce(-heldObject.linearVelocity * damper);
    }

    private void RotateHeldObject()
    {
        // Get mouse movement
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        // Apply torque based on camera's orientation
        // Rotate around camera's up axis for horizontal movement
        heldObject.AddTorque(mainCamera.transform.up * mouseX, ForceMode.VelocityChange);
        // Rotate around camera's right axis for vertical movement
        heldObject.AddTorque(-mainCamera.transform.right * mouseY, ForceMode.VelocityChange);
    }

    /// <summary>
    /// This is where you will disable/enable your camera's rotation script.
    /// </summary>
    /// <param name="canRotate">True to enable camera movement, false to disable.</param>
    private void ToggleCameraRotation(bool canRotate)
    {
        // Control the ExampleCharacterCamera component
        if (cameraController != null)
        {
            cameraController.SetRotationEnabled(canRotate);
        }

        // Control the ExampleCharacterController component
        if (characterController != null)
        {
            characterController.SetRotationEnabled(canRotate);
        }

        Debug.Log($"Camera and character rotation enabled: {canRotate}");
    }
}