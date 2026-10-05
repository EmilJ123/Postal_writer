using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimpleKicksled : MonoBehaviour
{
    [Header("Mount Setup")]
    [SerializeField] private Transform mountPoint;

    [Header("Kick Dynamics")]
    [SerializeField] private float kickForce = 12f;
    [SerializeField] private float kickCooldown = 0.8f; // Pause required between kicks
    [SerializeField] private float maxSpeed = 22f;

    [Header("Slide & Surface Physics")]
    [SerializeField] private float slideFriction = 0.4f; // Low ice/snow friction
    [SerializeField] private float slopeGravityMultiplier = 15f;
    [SerializeField] private float gravity = -15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Kicksled Steering & Inertia")]
    [SerializeField] private float turnTorque = 90f; // Turning strength
    [SerializeField] private float turnDrag = 3f;     // Angular resistance
    [SerializeField] private float runnerLateralFriction = 4f; // How hard runners resist sliding sideways

    private CharacterController controller;
    private Vector3 currentVelocity;
    private Vector2 moveInput;
    private bool isMounted = false;

    // Cooldown & Angular Dynamics
    private float lastKickTime = -999f;
    private float currentAngularVelocity = 0f;

    public Transform MountPoint => mountPoint != null ? mountPoint : transform;
    public bool CanKick => Time.time >= lastKickTime + kickCooldown;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public void SetMounted(bool mounted)
    {
        isMounted = mounted;
        if (!mounted)
        {
            moveInput = Vector2.zero;
            currentAngularVelocity = 0f;
        }
    }

    public void SetInput(Vector2 input)
    {
        moveInput = input;
    }

    public bool Kick()
    {
        if (!isMounted) return false;

        // Kick Cooldown Check
        if (Time.time < lastKickTime + kickCooldown)
        {
            return false; // Still in kick pause buffer
        }

        lastKickTime = Time.time;

        // Apply forward propulsion in current facing direction
        currentVelocity += transform.forward * kickForce;
        return true;
    }

    private void Update()
    {
        if (!isMounted) return;

        // --- 1. REALISTIC KICKSLED STEERING (Angular Inertia) ---
        float turnInput = moveInput.x;
        
        // Add steering torque
        currentAngularVelocity += turnInput * turnTorque * Time.deltaTime;
        
        // Apply angular drag (feels heavy and grounded)
        currentAngularVelocity = Mathf.Lerp(currentAngularVelocity, 0f, turnDrag * Time.deltaTime);
        
        // Rotate the sled
        transform.Rotate(0f, currentAngularVelocity * Time.deltaTime, 0f);


        // --- 2. OMNIDIRECTIONAL SLOPE SLIDING ---
        bool isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, out RaycastHit hit, 1.2f, groundLayer);

        if (isGrounded)
        {
            Vector3 groundNormal = hit.normal;
            
            // Calculate absolute downhill direction on the slope surface
            Vector3 downhillDirection = Vector3.ProjectOnPlane(Vector3.down, groundNormal).normalized;
            float slopeAngle = Vector3.Angle(groundNormal, Vector3.up);

            // Slide downhill regardless of facing orientation
            if (slopeAngle > 2f)
            {
                float slopeFactor = Mathf.Sin(slopeAngle * Mathf.Deg2Rad);
                currentVelocity += downhillDirection * (slopeFactor * slopeGravityMultiplier * Time.deltaTime);
            }

            // --- 3. RUNNER DIRECTIONAL FRICTION (Resists sideways skidding) ---
            Vector3 localVel = transform.InverseTransformDirection(currentVelocity);
            
            // Apply higher friction to lateral (side-to-side) velocity than forward/backward sliding
            localVel.x = Mathf.Lerp(localVel.x, 0f, runnerLateralFriction * Time.deltaTime);
            localVel.z = Mathf.Lerp(localVel.z, 0f, slideFriction * Time.deltaTime);

            currentVelocity = transform.TransformDirection(localVel);
        }
        else
        {
            // Air friction
            currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, slideFriction * 0.5f * Time.deltaTime);
        }


        // --- 4. SPEED CAP & MOVEMENT APPLY ---
        currentVelocity = Vector3.ClampMagnitude(currentVelocity, maxSpeed);

        Vector3 finalMove = currentVelocity;
        finalMove.y = gravity; // Continuous downward ground stickiness

        controller.Move(finalMove * Time.deltaTime);
    }
}