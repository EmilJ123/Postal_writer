using UnityEngine;
using UnityEngine.InputSystem;

public class S_Player_movement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.8f;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private bool shouldFaceMoveDirection = false;

    [Header("Components & Direct Interaction")]
    public PlayerInput playerInput;
    [SerializeField] private float mountDistance = 5f;
    [SerializeField] private Key interactKey = Key.E;

    private CharacterController controller;
    private Vector3 moveInput;
    private Vector3 velocity;

    // Kicksled State
    private S_Kicksled currentSled;
    private bool isOnKicksled = false;

    public bool IsOnKicksled => isOnKicksled;

    private void Start()
    {
        controller = GetComponent<CharacterController>();

        if (playerInput == null)
        {
            playerInput = GetComponent<PlayerInput>();
        }

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void Update()
    {
        // Check for mount attempt when on foot
        if (!isOnKicksled && Keyboard.current != null && Keyboard.current[interactKey].wasPressedThisFrame)
        {
            TryMount();
        }

        if (isOnKicksled) return;

        // Ground check
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // On-foot movement logic
        Vector3 forward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
        Vector3 right = cameraTransform != null ? cameraTransform.right : Vector3.right;

        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = forward * moveInput.y + right * moveInput.x;
        controller.Move(moveDirection * (speed * Time.deltaTime));

        if (shouldFaceMoveDirection && moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, 10f * Time.deltaTime);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // --- INPUT SYSTEM CALLBACKS (On Foot) ---

    public void Move(InputAction.CallbackContext context)
    {
        if (isOnKicksled) return;
        moveInput = context.ReadValue<Vector2>();
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (isOnKicksled) return;

        if (context.performed && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    public void Interact(InputAction.CallbackContext context)
    {
        if (!context.performed || isOnKicksled) return;
        TryMount();
    }

    // --- MOUNT / DISMOUNT LOGIC ---

    private void TryMount()
    {
        S_Kicksled[] sleds = FindObjectsByType<S_Kicksled>(FindObjectsInactive.Exclude);

        foreach (S_Kicksled sled in sleds)
        {
            float dist = Vector3.Distance(transform.position, sled.transform.position);

            if (dist <= mountDistance)
            {
                currentSled = sled;
                isOnKicksled = true;

                // Deactivate CharacterController & PlayerInput
                controller.enabled = false;
                if (playerInput != null)
                {
                    playerInput.DeactivateInput();
                    playerInput.enabled = false;
                }

                // Attach player transform to sled mount point
                transform.SetParent(sled.MountPoint);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;

                // Pass player instance to sled
                sled.Mount(this);
                Debug.Log("<color=green>[Kicksled]</color> Mounted! PlayerInput is DEACTIVATED.");
                break;
            }
        }
    }

    public void Dismount()
    {
        if (!isOnKicksled) return;

        isOnKicksled = false;

        // Save reference position to offset player safely to the left of the sled on dismount
        Vector3 dismountPos = currentSled != null ? currentSled.transform.position - currentSled.transform.right * 1.2f : transform.position;

        currentSled = null;

        // Detach player transform
        transform.SetParent(null);
        transform.position = dismountPos;

        // Reactivate CharacterController & PlayerInput
        controller.enabled = true;
        if (playerInput != null)
        {
            playerInput.enabled = true;
            playerInput.ActivateInput();
        }

        Debug.Log("<color=yellow>[Kicksled]</color> Dismounted successfully! PlayerInput REACTIVATED.");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, mountDistance);
    }
}