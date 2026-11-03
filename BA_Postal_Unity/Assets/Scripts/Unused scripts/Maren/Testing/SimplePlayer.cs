using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class SimplePlayer : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private Transform cameraTransform;

    [Header("Interaction Settings")]
    [SerializeField] private float mountDistance = 5f;
    [SerializeField] private Key fallbackInteractKey = Key.E;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 velocity;

    // Mount State
    private SimpleKicksled currentSled;
    private bool isMounted = false;

    public bool IsMounted => isMounted;

    private void Start()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void Update()
    {
        // Direct Keyboard Fallbacks
        if (Keyboard.current != null)
        {
            if (Keyboard.current[fallbackInteractKey].wasPressedThisFrame)
            {
                ToggleMount();
            }

            if (isMounted && Keyboard.current[Key.Space].wasPressedThisFrame)
            {
                TryKickSled();
            }
        }

        // Normal Walking Logic
        if (isMounted) return;

        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        Vector3 forward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
        Vector3 right = cameraTransform != null ? cameraTransform.right : Vector3.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = forward * moveInput.y + right * moveInput.x;
        controller.Move(moveDirection * (walkSpeed * Time.deltaTime));

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDirection), 10f * Time.deltaTime);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void TryKickSled()
    {
        if (currentSled != null)
        {
            bool kicked = currentSled.Kick();
            if (kicked)
            {
                Debug.Log("<color=green>[KICKSLED] Kick executed!</color>");
            }
        }
    }

    public void ToggleMount()
    {
        if (isMounted)
        {
            Dismount();
        }
        else
        {
            TryMount();
        }
    }

    private void TryMount()
    {
        SimpleKicksled[] sleds = FindObjectsByType<SimpleKicksled>(FindObjectsInactive.Exclude);

        foreach (SimpleKicksled sled in sleds)
        {
            float dist = Vector3.Distance(transform.position, sled.transform.position);

            if (dist <= mountDistance)
            {
                currentSled = sled;
                isMounted = true;

                controller.enabled = false;
                transform.SetParent(sled.MountPoint);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;

                sled.SetMounted(true);
                Debug.Log("<color=green>Mounted Kicksled.</color>");
                break;
            }
        }
    }

    private void Dismount()
    {
        if (currentSled == null) return;

        currentSled.SetMounted(false);
        currentSled = null;

        transform.SetParent(null);
        controller.enabled = true;
        isMounted = false;
        Debug.Log("<color=yellow>Dismounted from Kicksled.</color>");
    }

    // --- NEW INPUT SYSTEM CALLBACKS ---

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        if (isMounted && currentSled != null)
        {
            currentSled.SetInput(moveInput);
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (isMounted)
        {
            TryKickSled();
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        ToggleMount();
    }
}