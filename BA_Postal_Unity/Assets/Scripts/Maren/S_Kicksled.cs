using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class S_Kicksled : MonoBehaviour
{
    [Header("Mount Setup")]
    [SerializeField] private Transform mountPoint;

    [Header("Controls & Dismount Safety")]
    [SerializeField] private Key kickKey = Key.Space;
    [SerializeField] private Key dismountKey = Key.E;
    [Tooltip("Maximum speed allowed to safely dismount the sled.")]
    [SerializeField] private float dismountSpeedThreshold = 2f; 

    [Header("Kick Dynamics")]
    [SerializeField] private float kickForce = 14f;
    [SerializeField] private float kickCooldown = 0.6f;
    [SerializeField] private float maxSpeed = 22f;

    [Header("Slide & Surface Physics")]
    [SerializeField] private float slideFriction = 0.5f;
    [SerializeField] private float slopeGravityMultiplier = 18f;
    [SerializeField] private float gravity = -15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Kicksled Steering & Inertia")]
    [SerializeField] private float turnTorque = 120f;
    [SerializeField] private float turnDrag = 4f;
    [SerializeField] private float runnerLateralFriction = 5f;

    private CharacterController controller;
    private Vector3 currentVelocity;
    private S_Player_movement mountedPlayer;
    private bool isMounted = false;

    private float lastKickTime = -999f;
    private float currentAngularVelocity = 0f;
    private bool justMountedThisFrame = false;

    public Transform MountPoint => mountPoint != null ? mountPoint : transform;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public void Mount(S_Player_movement player)
    {
        isMounted = true;
        mountedPlayer = player;
        currentAngularVelocity = 0f;
        justMountedThisFrame = true; // Prevents frame-1 accidental dismounts
    }

    private void Update()
    {
        if (!isMounted) return;

        // Skip input on the exact frame of mounting
        if (justMountedThisFrame)
        {
            justMountedThisFrame = false;
            return;
        }

        // --- 1. DISMOUNT & KICK INPUTS ---
        if (Keyboard.current != null)
        {
            if (Keyboard.current[dismountKey].wasPressedThisFrame)
            {
                TryDismount();
                if (!isMounted) return; // Exit update early if dismounted successfully
            }

            if (Keyboard.current[kickKey].wasPressedThisFrame)
            {
                TryKick();
            }
        }

        // --- 2. STEERING (A / D) ---
        float turnInput = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current[Key.A].isPressed || Keyboard.current[Key.LeftArrow].isPressed) turnInput -= 1f;
            if (Keyboard.current[Key.D].isPressed || Keyboard.current[Key.RightArrow].isPressed) turnInput += 1f;
        }

        currentAngularVelocity += turnInput * turnTorque * Time.deltaTime;
        currentAngularVelocity = Mathf.Lerp(currentAngularVelocity, 0f, turnDrag * Time.deltaTime);
        transform.Rotate(0f, currentAngularVelocity * Time.deltaTime, 0f);

        // --- 3. SLOPE SLIDING & SURFACE PHYSICS ---
        bool isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, out RaycastHit hit, 1.2f, groundLayer);

        if (isGrounded)
        {
            Vector3 groundNormal = hit.normal;
            Vector3 downhillDirection = Vector3.ProjectOnPlane(Vector3.down, groundNormal).normalized;
            float slopeAngle = Vector3.Angle(groundNormal, Vector3.up);

            if (slopeAngle > 2f)
            {
                float slopeFactor = Mathf.Sin(slopeAngle * Mathf.Deg2Rad);
                currentVelocity += downhillDirection * (slopeFactor * slopeGravityMultiplier * Time.deltaTime);
            }

            // --- 4. RUNNER FRICTION & NATURAL DECELERATION ---
            Vector3 localVel = transform.InverseTransformDirection(currentVelocity);
            localVel.x = Mathf.Lerp(localVel.x, 0f, runnerLateralFriction * Time.deltaTime);
            localVel.z = Mathf.Lerp(localVel.z, 0f, slideFriction * Time.deltaTime);

            currentVelocity = transform.TransformDirection(localVel);
        }
        else
        {
            currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, slideFriction * 0.5f * Time.deltaTime);
        }

        // --- 5. APPLY MOVEMENT ---
        currentVelocity = Vector3.ClampMagnitude(currentVelocity, maxSpeed);

        Vector3 finalMove = currentVelocity;
        finalMove.y = gravity;

        controller.Move(finalMove * Time.deltaTime);
    }

    private void TryKick()
    {
        if (Time.time < lastKickTime + kickCooldown) return;

        lastKickTime = Time.time;
        currentVelocity += transform.forward * kickForce;
        Debug.Log("<color=cyan>[Kicksled]</color> KICK!");
    }

    private void TryDismount()
    {
        // Check horizontal speed (ignoring vertical gravity)
        float currentHorizontalSpeed = new Vector3(currentVelocity.x, 0f, currentVelocity.z).magnitude;

        if (currentHorizontalSpeed > dismountSpeedThreshold)
        {
            Debug.LogWarning($"<color=orange>[Kicksled]</color> Moving too fast to dismount safely! Current Speed: {currentHorizontalSpeed:F1} / Max Allowed: {dismountSpeedThreshold:F1}");
            return;
        }

        Dismount();
    }

    private void Dismount()
    {
        isMounted = false;

        if (mountedPlayer != null)
        {
            S_Player_movement playerToDismount = mountedPlayer;
            mountedPlayer = null;
            playerToDismount.Dismount();
        }
    }
}