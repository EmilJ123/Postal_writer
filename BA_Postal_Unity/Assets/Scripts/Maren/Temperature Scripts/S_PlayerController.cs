using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;


[RequireComponent(typeof(CharacterController))]
public class S_PlayerController : MonoBehaviour
{
    [Header("Whole Number Vitals")]
    public int maxHealth = 100;
    public int currentHealth = 100;
    public bool isDead = false;


    [Header("Stamina Pool (Clean Integers)")]
    //public int maxStamina = 100;
    //public int currentStamina = 100;
    //public int sprintDrainRate = 20; // Stamina lost per second of sprinting
    //public int staminaRegenRate = 12; // Stamina gained per second of resting
    //public int shoveStaminaCost = 30;


    [Header("Exhaustion Logic (Haunting Ground Style)")]
    //public bool isExhausted = false;
    [Tooltip("Stamina must reach this integer value to clear exhaustion")]
    //public int recoveryThreshold = 40;


    [Header("Locomotion Configuration")]
    public float walkSpeed = 3.5f;
    public float sprintSpeed = 6.5f;
    public float rotationSpeed = 140f;
    public float gravity = 9.81f;


    [Header("Defensive Shove & Interaction")]
    //public float shoveRadius = 1.8f;
    //public float shoveForce = 5f;
    public float interactRadius = 1.5f;
    //public bool hasCrowbar = false;
    //public List<KeyItemData> keyRingInventory = new List<KeyItemData>();


    private CharacterController controller;
    private float verticalVelocity;
    //private DoorController currentActiveDoor;
    private float currentMoveSpeed;


    // Internal time accumulators to guarantee whole-number-only execution
    //private float staminaTickTimer = 0f;


    void Start()
    {
        controller = GetComponent<CharacterController>();
        currentHealth = maxHealth;
        //currentStamina = maxStamina;
    }
    


    private void HandleMovementAndStamina()
    {
        // 1. Tank Rotation Mechanics
        float turnInput = 0f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) turnInput = 1f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) turnInput = -1f;
        transform.Rotate(0, turnInput * rotationSpeed * Time.deltaTime, 0);


        // 2. Sprint Intent Verification
        float forwardInput = 0f;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) forwardInput = 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) forwardInput = -1f;


        bool wantsToSprint = Keyboard.current.leftShiftKey.isPressed && forwardInput > 0.1f;


        // --- FIXED: GET THE STATE LAYER FROM TEMPERATURE SYSTEM ---
        S_TemperatureSystem tempSystem = GetComponent<S_TemperatureSystem>();
        bool isHypothermic = (tempSystem != null && tempSystem.isFreezing);


        // 4. Transform Position Application
        Vector3 moveVector = transform.forward * (forwardInput * currentMoveSpeed);
        if (controller.isGrounded) verticalVelocity = -0.5f;
        else verticalVelocity -= gravity * Time.deltaTime;
        moveVector.y = verticalVelocity;
       
        controller.Move(moveVector * Time.deltaTime);
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        Debug.LogError($"<color=red>[Damage Taken]</color> Voss HP: {currentHealth}/{maxHealth}");


        if (currentHealth <= 0)
        {
            isDead = true;
            Debug.LogError("GAME OVER: Voss froze in the dark.");
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shoveRadius);
    }
}
