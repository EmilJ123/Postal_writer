using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SphereCollider))]
public class S_LanternSystem : MonoBehaviour
{
    [Header("Integer Fuel Configuration")]
    public int maxFuel = 100;
    public int currentFuel = 100;
    public int burnRatePerSecond = 2; // Subtracts exactly this many units per second
    public bool isEquipped = true;
    [Header("Dynamic Thermal Settings")]
    public bool isActive = true;
   
    [Tooltip("Maximum heat points restored per second when standing directly against the object.")]
    public int maxWarmthGeneration = 12;


    [Tooltip("This should match the radius of your Sphere Collider trigger for accurate distance falloff calculations.")]
    public float heatRadius = 4f;

    [Header("Visual Assignments")]
    public GameObject lanternLightObject;
    private SphereCollider sphereCollider;

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
