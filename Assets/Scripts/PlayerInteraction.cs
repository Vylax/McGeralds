using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
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

    [Header("Input Settings")]
    [SerializeField] private InputActionAsset inputActions;

    private Rigidbody heldObject;
    private Camera mainCamera;
    private bool isRotatingObject = false;
    
    // References to camera and character controllers
    private ExampleCharacterCamera cameraController;
    private ExampleCharacterController characterController;
    
    // Input Actions
    private InputAction interactAction;
    private InputAction attackAction;
    private InputAction lookAction;
    private InputAction rotateObjectAction;

    void Start()
    {
        // Initialize Input System
        SetupInputActions();
        
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

    void SetupInputActions()
    {
        // Load the Input Actions asset if not assigned
        if (inputActions == null)
        {
            inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");
            if (inputActions == null)
            {
                Debug.LogError("InputSystem_Actions asset not found! Please assign it in the inspector or place it in a Resources folder.");
                return;
            }
        }
        
        // Get the Player action map
        var playerActionMap = inputActions.FindActionMap("Player");
        if (playerActionMap == null)
        {
            Debug.LogError("Player action map not found!");
            return;
        }
        
        // Initialize individual actions
        interactAction = playerActionMap.FindAction("Interact");
        attackAction = playerActionMap.FindAction("Attack");
        lookAction = playerActionMap.FindAction("Look");
        rotateObjectAction = playerActionMap.FindAction("RotateObject");
        
        // Log warnings for missing actions
        if (interactAction == null) Debug.LogError("Interact action not found!");
        if (attackAction == null) Debug.LogError("Attack action not found!");
        if (lookAction == null) Debug.LogError("Look action not found!");
        if (rotateObjectAction == null) Debug.LogError("RotateObject action not found!");
    }

    void OnEnable()
    {
        interactAction?.Enable();
        attackAction?.Enable();
        lookAction?.Enable();
        rotateObjectAction?.Enable();
    }

    void OnDisable()
    {
        interactAction?.Disable();
        attackAction?.Disable();
        lookAction?.Disable();
        rotateObjectAction?.Disable();
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
        // Press E (Interact) to drop
        if (interactAction != null && interactAction.WasPressedThisFrame())
        {
            DropObject();
            return;
        }

        // Left-click (Attack) to throw
        if (attackAction != null && attackAction.WasPressedThisFrame())
        {
            ThrowObject();
            return;
        }

        // Right-click to start rotating
        if (rotateObjectAction != null && rotateObjectAction.WasPressedThisFrame())
        {
            isRotatingObject = true;
            ToggleCameraRotation(false); // Disable camera look
        }
        // Right-click release to stop rotating
        if (rotateObjectAction != null && rotateObjectAction.WasReleasedThisFrame())
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

        if (isLookingAtGrabbable && interactAction != null && interactAction.WasPressedThisFrame())
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
        // Get mouse movement from the Look action
        if (lookAction != null)
        {
            Vector2 lookInput = lookAction.ReadValue<Vector2>();
            float mouseX = lookInput.x * rotationSpeed * Time.fixedDeltaTime;
            float mouseY = lookInput.y * rotationSpeed * Time.fixedDeltaTime;

            // Apply torque based on camera's orientation
            // Rotate around camera's up axis for horizontal movement
            heldObject.AddTorque(mainCamera.transform.up * mouseX, ForceMode.VelocityChange);
            // Rotate around camera's right axis for vertical movement
            heldObject.AddTorque(-mainCamera.transform.right * mouseY, ForceMode.VelocityChange);
        }
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