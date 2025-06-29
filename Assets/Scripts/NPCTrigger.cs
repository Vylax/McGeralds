using UnityEngine;
using UnityEngine.InputSystem;

public class NPCTrigger : MonoBehaviour
{
    public DynamicSpeechBubble npcToTalkTo;
    public string dialogueLine;
    public AudioClip voiceLine;
    
    [Header("Input Settings")]
    [SerializeField] private InputActionAsset inputActions;
    private InputAction debugAction;
    private string currentKeyboardLayout = "";

    void Awake()
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
        
        // Get the Debug action from the Player action map
        var playerActionMap = inputActions.FindActionMap("Player");
        if (playerActionMap != null)
        {
            debugAction = playerActionMap.FindAction("Debug");
        }
        
        if (debugAction == null)
        {
            Debug.LogError("Debug action not found in Player action map!");
            return;
        }

        // Setup keyboard layout detection and binding
        SetupKeyboardLayoutBinding();
        
        // Listen for layout changes
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    void OnDestroy()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    void SetupKeyboardLayoutBinding()
    {
        if (debugAction == null || Keyboard.current == null) return;

        // Get current keyboard layout
        string layout = Keyboard.current.keyboardLayout.ToLower();
        
        // Only update if layout has changed
        if (currentKeyboardLayout == layout) return;
        
        currentKeyboardLayout = layout;
        
        // Clear existing bindings
        debugAction.Disable();
        for (int i = debugAction.bindings.Count - 1; i >= 0; i--)
        {
            debugAction.ChangeBinding(i).Erase();
        }
        
        // Determine which key to bind based on layout
        string keyToBind;
        if (IsAzertyLayout(layout))
        {
            keyToBind = "<Keyboard>/a";
            Debug.Log("AZERTY keyboard detected - Debug action bound to 'A' key");
        }
        else
        {
            keyToBind = "<Keyboard>/q"; 
            Debug.Log($"QWERTY keyboard detected (Layout: {layout}) - Debug action bound to 'Q' key");
        }
        
        // Add the appropriate binding
        debugAction.AddBinding(keyToBind);
        
        // Re-enable the action
        if (gameObject.activeInHierarchy)
        {
            debugAction.Enable();
        }
    }

    bool IsAzertyLayout(string layout)
    {
        // Check for common AZERTY layout identifiers
        return layout.Contains("azerty") || 
               layout.Contains("french") || 
               layout.Contains("fr-") ||
               layout.Contains("belgian") ||
               layout.Contains("be-");
    }

    void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        // Re-setup bindings when keyboard configuration changes
        if (device is Keyboard && change == InputDeviceChange.ConfigurationChanged)
        {
            SetupKeyboardLayoutBinding();
        }
    }

    void OnEnable()
    {
        debugAction?.Enable();
    }

    void OnDisable()
    {
        debugAction?.Disable();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            npcToTalkTo.Talk(dialogueLine, voiceLine);
        }
    }

    private void Update()
    {
        //debug only - now works with A for AZERTY and Q for QWERTY keyboards
        if (debugAction != null && debugAction.WasPressedThisFrame())
        {
            npcToTalkTo.Talk(dialogueLine, voiceLine);

        }
    }
}