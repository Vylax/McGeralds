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
        // --- SAFETY CHECK: If held object was destroyed externally, clean up ---
        if (heldObject != null && heldObject.gameObject == null)
        {
            CleanupHeldObject();
        }

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
        // --- SAFETY CHECK: Ensure held object still exists ---
        if (heldObject != null && heldObject.gameObject == null)
        {
            CleanupHeldObject();
            return;
        }

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
    /// Cleans up the held object reference when it has been destroyed externally.
    /// </summary>
    private void CleanupHeldObject()
    {
        Debug.Log("Held object was destroyed externally. Cleaning up references.");
        
        // If we were rotating, make sure to re-enable the camera
        if (isRotatingObject)
        {
            isRotatingObject = false;
            ToggleCameraRotation(true);
        }

        heldObject = null;
        
        // Hide interaction prompt if it's showing
        if (interactionPrompt != null)
        {
            interactionPrompt.gameObject.SetActive(false);
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
    /// Checks if the player is looking at an NPC or grabbable object and handles interactions.
    /// NPCs take priority over grabbable objects.
    /// </summary>
    private void CheckForGrabbableObject()
    {
        RaycastHit hit;
        bool hitSomething = Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out hit, interactionDistance);
        
        // Check for NPC first (priority over grabbable objects)
        NPCTrigger npcTrigger = null;
        bool isLookingAtNPC = false;
        if (hitSomething)
        {
            npcTrigger = hit.collider.GetComponent<NPCTrigger>();
            isLookingAtNPC = npcTrigger != null;
        }
        
        // Check for grabbable object (only if not looking at NPC)
        bool isLookingAtGrabbable = false;
        if (hitSomething && !isLookingAtNPC)
        {
            isLookingAtGrabbable = hit.collider.attachedRigidbody != null
                                   && !hit.collider.CompareTag("Player")
                                   && !hit.collider.CompareTag("Map");
        }

        // Show interaction prompt if looking at either NPC or grabbable object
        if (interactionPrompt != null)
        {
            interactionPrompt.gameObject.SetActive(isLookingAtNPC || isLookingAtGrabbable);
        }

        // Handle interactions when interact key is pressed
        if (interactAction != null && interactAction.WasPressedThisFrame())
        {
            if (isLookingAtNPC && npcTrigger != null)
            {
                // Handle NPC interaction
                string dialogueToUse = GetNPCDialogue(npcTrigger);
                if (npcTrigger.npcToTalkTo != null)
                {
                    npcTrigger.npcToTalkTo.Talk(dialogueToUse, npcTrigger.voiceLine);
                }
            }
            else if (isLookingAtGrabbable)
            {
                // Handle object grabbing
                TryGrabObject(hit);
            }
        }
    }
    
    /// <summary>
    /// Gets the appropriate dialogue for an NPC based on their settings.
    /// </summary>
    private string GetNPCDialogue(NPCTrigger npcTrigger)
    {
        if (npcTrigger.useCustomDialogue)
        {
            return npcTrigger.dialogueLine;
        }
        else
        {
            // Use random dialogue from preset lines
            string[] randomDialogueLines = {
                "Why is the ice cream machine ALWAYS broken?!",
                "I just want an ice cream, is that too much to ask?",
                "Excuse me, is your ice cream machine working today?",
                "I've been to 3 McGerald's and none of them have working ice cream!",
                "Can I get a vanilla cone? Oh wait, let me guess... machine's broken?",
                "I specifically came here for ice cream and now you tell me it's broken?",
                "How hard is it to fix an ice cream machine? Seriously!",
                "I'm never coming back here again! ...until tomorrow for ice cream.",
                "Do you guys even TRY to fix the machine or just leave it broken?",
                "I bet if I worked here I could fix that machine in 5 minutes!",
                "My kid is crying because they want ice cream and your machine is broken AGAIN!",
                "Is there a conspiracy against McGerald's ice cream or something?",
                "I drove 20 minutes just for an ice cream and you're telling me no?",
                "Can't you just go to the store and buy some ice cream to sell?",
                "I'm calling corporate about this broken machine situation!",
                "Why don't you put a sign outside saying 'Ice Cream Machine Broken'?",
                "I'll take anything cold... a frozen burger, I don't care anymore!",
                "Is the machine actually broken or are you just too lazy to clean it?",
                "I bet the ice cream machine at Burger Emperor works!",
                "Can I speak to the manager about this ice cream situation?",
                "I just want to know WHY it's always broken!",
                "Do you have any ice cream in the back freezer I could buy?",
                "I'm starting to think McGerald's ice cream is just a myth!",
                "Next time I'm bringing my own ice cream to eat here!",
                "How am I supposed to enjoy my fries without a McGerald's ice cream?!"
            };
            
            if (randomDialogueLines.Length > 0)
            {
                int randomIndex = Random.Range(0, randomDialogueLines.Length);
                return randomDialogueLines[randomIndex];
            }
            else
            {
                return "Hello there!"; // Fallback dialogue
            }
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