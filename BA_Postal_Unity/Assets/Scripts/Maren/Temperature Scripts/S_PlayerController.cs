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
    public int maxStamina = 100;
    public int currentStamina = 100;
    public int sprintDrainRate = 20; // Stamina lost per second of sprinting
    public int staminaRegenRate = 12; // Stamina gained per second of resting
    public int shoveStaminaCost = 30;


    [Header("Exhaustion Logic (Haunting Ground Style)")]
    public bool isExhausted = false;
    [Tooltip("Stamina must reach this integer value to clear exhaustion")]
    public int recoveryThreshold = 40;


    [Header("Locomotion Configuration")]
    public float walkSpeed = 3.5f;
    public float sprintSpeed = 6.5f;
    public float rotationSpeed = 140f;
    public float gravity = 9.81f;


    [Header("Defensive Shove & Interaction")]
    public float shoveRadius = 1.8f;
    public float shoveForce = 5f;
    public float interactRadius = 1.5f;
    public bool hasCrowbar = false;
    public List<KeyItemData> keyRingInventory = new List<KeyItemData>();


    private CharacterController controller;
    private float verticalVelocity;
    private DoorController currentActiveDoor;
    private float currentMoveSpeed;


    // Internal time accumulators to guarantee whole-number-only execution
    private float staminaTickTimer = 0f;


    void Start()
    {
        controller = GetComponent<CharacterController>();
        currentHealth = maxHealth;
        currentStamina = maxStamina;
    }


    void Update()
    {
        if (isDead || Keyboard.current == null) return;


        HandleMovementAndStamina();


        // Action: Spacebar Shove
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ExecuteDefensiveShove();
        }


        // Action: Proximity Interaction
        if (Keyboard.current.eKey.isPressed)
        {
            EvaluateProximityInteraction();
        }
        else if (currentActiveDoor != null)
        {
            currentActiveDoor.ResetPryProgress();
            currentActiveDoor = null;
        }
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
        TemperatureSystem tempSystem = GetComponent<TemperatureSystem>();
        bool isHypothermic = (tempSystem != null && tempSystem.isFreezing);


        // Exhaustion AND Hypothermia State Gate Check
        if (isExhausted || isHypothermic)
        {
            if (wantsToSprint)
            {
                if (isHypothermic) Debug.Log("<color=cyan>[Movement Blocked]</color> Voss tries to run, but her frozen joints collapse into a slow crawl!");
                else Debug.Log("<color=orange>[Movement Blocked]</color> Voss is completely out of breath!");
            }
           
            wantsToSprint = false; // System completely forces sprinting to fail
           
            if (currentStamina >= recoveryThreshold && !isHypothermic)
            {
                isExhausted = false;
                Debug.Log("<color=green>[Stamina]</color> Voss caught her breath. Sprinting re-enabled.");
            }
        }


        bool isSprinting = wantsToSprint && currentStamina > 0;


        // Set speed values based on state calculations
        currentMoveSpeed = isSprinting ? sprintSpeed : walkSpeed;


        // 3. Whole Number Stamina Tick Engine
        staminaTickTimer += Time.deltaTime;


        if (isSprinting)
        {
            float interval = 1f / sprintDrainRate;
            if (staminaTickTimer >= interval)
            {
                currentStamina = Mathf.Clamp(currentStamina - 1, 0, maxStamina);
                staminaTickTimer = 0f;


                if (currentStamina <= 0)
                {
                    isExhausted = true;
                    Debug.LogWarning("<color=red>[Stamina]</color> Voss is completely exhausted! She's out of breath!");
                }
            }
        }
        else // Regenerating Stamina while walking or standing still
        {
            if (currentStamina < maxStamina)
            {
                float interval = 1f / staminaRegenRate;
                if (staminaTickTimer >= interval)
                {
                    currentStamina = Mathf.Clamp(currentStamina + 1, 0, maxStamina);
                    staminaTickTimer = 0f;
                }
            }
            else
            {
                staminaTickTimer = 0f;
            }
        }


        // 4. Transform Position Application
        Vector3 moveVector = transform.forward * forwardInput * currentMoveSpeed;
        if (controller.isGrounded) verticalVelocity = -0.5f;
        else verticalVelocity -= gravity * Time.deltaTime;
        moveVector.y = verticalVelocity;
       
        controller.Move(moveVector * Time.deltaTime);
    }


    private void ExecuteDefensiveShove()
    {
        if (currentStamina < shoveStaminaCost || isExhausted)
        {
            Debug.LogWarning("<color=orange>[Action Blocked]</color> Voss is breathing too heavily to attempt a shove!");
            return;
        }


        currentStamina = Mathf.Clamp(currentStamina - shoveStaminaCost, 0, maxStamina);
        if (currentStamina <= 0) isExhausted = true;


        Debug.Log("<color=yellow>[Combat]</color> Voss shoves forcefully!");


        Collider[] hitColliders = Physics.OverlapSphere(transform.position, shoveRadius);
        foreach (var col in hitColliders)
        {
            CryophorusAI enemy = col.GetComponent<CryophorusAI>();
            if (enemy != null)
            {
                enemy.GetShoved(shoveForce);
            }
        }
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


    private void EvaluateProximityInteraction()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactRadius);
        DoorController foundDoor = null;
       
        foreach (var hitCollider in hitColliders)
        {
            // --- NEW: TALLOW STATION INTERACTION HOOK ---
            TallowStation station = hitCollider.GetComponent<TallowStation>();
            if (station != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                station.TryRefillLantern(this);
                return; // Action consumed
            }


            KeyPickup pickup = hitCollider.GetComponent<KeyPickup>();
            if (pickup != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                pickup.CollectPickup(this);
                return;
            }


            MusicBoxPuzzle puzzle = hitCollider.GetComponent<MusicBoxPuzzle>();
            if (puzzle != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                puzzle.AttemptSolve(this);
                return;
            }


            DoorController door = hitCollider.GetComponent<DoorController>();
            if (door != null) foundDoor = door;
        }


        if (foundDoor != null)
        {
            currentActiveDoor = foundDoor;
            currentActiveDoor.PlayerInteract(this);
        }
    }


    public void AddKeyToInventory(KeyItemData newKey) { if (!keyRingInventory.Contains(newKey)) keyRingInventory.Add(newKey); }
    public KeyItemData FindKeyInInventory(string targetID) { return keyRingInventory.Find(key => key.keyID == targetID); }
    public void RemoveKeyFromInventory(KeyItemData keyToRemove) { if (keyRingInventory.Contains(keyToRemove)) keyRingInventory.Remove(keyToRemove); }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shoveRadius);
    }
}
