using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class S_ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float zoomLerpSpeed = 10f;
    [SerializeField] private float minDistance = 3f;
    [SerializeField] private float maxDistance = 15f;

    // Drag your input action asset file directly here in the Inspector
    [Header("Input Setup")]
    [SerializeField] private InputActionReference mouseZoomAction;

    private CinemachineCamera cam;
    private CinemachineOrbitalFollow orbital;
    private Vector2 scrollDelta;

    private float targetZoom;
    private float currentZoom;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        
        cam = GetComponent<CinemachineCamera>();
        orbital = GetComponent<CinemachineOrbitalFollow>();

        if (orbital != null)
        {
            targetZoom = currentZoom = orbital.Radius;
        }
    }

    private void OnEnable()
    {
        // Safely bind to the input action asset and ensure it's turned on
        if (mouseZoomAction != null)
        {
            mouseZoomAction.action.Enable();
            mouseZoomAction.action.performed += HandleMouseScroll;
        }
    }

    private void OnDisable()
    {
        // Unbind to prevent Fast Enter Play Mode memory leaks in Unity 6.6
        if (mouseZoomAction != null)
        {
            mouseZoomAction.action.performed -= HandleMouseScroll;
        }
    }

    private void HandleMouseScroll(InputAction.CallbackContext context)
    {
        scrollDelta = context.ReadValue<Vector2>();
        Debug.Log($"Mouse is scrolling. value: {scrollDelta}");
    }

    void Update()
    {
        // scrollDelta.y handles positive (scroll up) and negative (scroll down)
        if (scrollDelta.y != 0 && orbital != null)
        {
            targetZoom = Mathf.Clamp(orbital.Radius - scrollDelta.y * zoomSpeed, minDistance, maxDistance);
            scrollDelta = Vector2.zero; 
        }
        
        if (orbital != null)
        {
            currentZoom = Mathf.Lerp(currentZoom, targetZoom, Time.deltaTime * zoomLerpSpeed);
            orbital.Radius = currentZoom;
        }
    }
}
