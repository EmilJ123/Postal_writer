using UnityEngine;
using UnityEngine.InputSystem;


public class S_LanternSystem : MonoBehaviour
{
    [Header("Integer Fuel Configuration")]
    public int maxFuel = 100;
    public int currentFuel = 100;
    public int burnRatePerSecond = 2; // Subtracts exactly this many units per second
    public bool isEquipped = true;


    [Header("Visual Assignments")]
    public GameObject lanternLightObject;


    private float fuelTimer = 0f;
    private float darknessStressTimer = 0f;


    void Start()
    {
        currentFuel = maxFuel;
        UpdateVisualState();
    }


    void Update()
    {
        if (Keyboard.current == null) return;


        // Toggle Lantern Equip (F Key)
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            isEquipped = !isEquipped;
            UpdateVisualState();
            Debug.Log(isEquipped ? "<color=yellow>[Lantern]</color> Raised." : "<color=gray>[Lantern]</color> Lowered.");
        }


        // Integer Fuel Tick Engine
        if (isEquipped && currentFuel > 0)
        {
            fuelTimer += Time.deltaTime;
            float targetInterval = 1f / burnRatePerSecond;


            if (fuelTimer >= targetInterval)
            {
                currentFuel = Mathf.Clamp(currentFuel - 1, 0, maxFuel);
                fuelTimer = 0f;


                if (currentFuel <= 0)
                {
                    isEquipped = false;
                    UpdateVisualState();
                    Debug.LogWarning("<color=blue>[Darkness]</color> Fuel depleted.");
                }
            }
        }


        // Integrity Penalty for standing unprotected in pitch black tunnels
        if (!isEquipped)
        {
            darknessStressTimer += Time.deltaTime;
            if (darknessStressTimer >= 1.0f) // Every 1 second of complete darkness
            {
                darknessStressTimer = 0f;
                if (IrisEmotionalMatrix.Instance != null)
                {
                    // Darkness passively increases environmental anxiety stress
                    IrisEmotionalMatrix.Instance.InduceAnxiety(1);
                }
            }
        }
    }


    private void UpdateVisualState()
    {
        if (lanternLightObject != null)
        {
            lanternLightObject.SetActive(isEquipped);
        }
    }


    public void AddTallow(int amount)
    {
        currentFuel = Mathf.Clamp(currentFuel + amount, 0, maxFuel);
    }
}
