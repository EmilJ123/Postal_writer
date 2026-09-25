using UnityEngine;
using UnityEngine.UI;


public class S_TemperatureSystem : MonoBehaviour
{
    [Header("Core Temperature (Clean Integers)")]
    public int maxTemperature = 100;
    public int currentTemperature = 100;
    public bool isFreezing = false;


    [Header("Visual Feedback HUD")]
    public Image frostOverlayImage;


    // Active Environment References
    private S_ClimateZone currentRoom;
    private S_HeatSource activeNearbyHeatSource;


    private S_PlayerController player;
    private S_LanternSystem lantern;


    // Thermal accumulation engines to guarantee strict integer value conversion
    private float freezeAccumulator = 0f;
    private float thawAccumulator = 0f;
    private float damageTimer = 0f;


    void Start()
    {
        currentTemperature = maxTemperature;
        player = GetComponent<S_PlayerController>();
        lantern = GetComponent<S_LanternSystem>();
        if (frostOverlayImage != null) UpdateFrostVisuals();
    }


    void Update()
    {
        if (player != null && player.isDead) return;


        bool isAbsorbingFireHeat = (activeNearbyHeatSource != null && activeNearbyHeatSource.isActive);


        if (isAbsorbingFireHeat)
        {
            ProcessProximityThawing();
        }
        else
        {
            ProcessClimateFreezing();
        }


        EvaluateTemperaturePenalties();
        UpdateFrostVisuals();
    }


    public void RegisterCurrentRoom(S_ClimateZone zone) { currentRoom = zone; }
    public void UnregisterCurrentRoom(S_ClimateZone zone) { if (currentRoom == zone) currentRoom = null; }
    public void RegisterHeatSource(S_HeatSource source) { activeNearbyHeatSource = source; }
    public void UnregisterHeatSource(S_HeatSource source) { if (activeNearbyHeatSource == source) activeNearbyHeatSource = null; }


    private void ProcessClimateFreezing()
    {
        thawAccumulator = 0f; // Flush warmth energy inputs
       
        bool isLanternLit = (lantern != null && lantern.isEquipped && lantern.currentFuel > 0);
       
        // Environment Evaluation: If no zone component is found, default to open freezing mines
        bool inColdZone = (currentRoom == null || !currentRoom.isWarmRoom);
       
        //bool isIrisHysterical = (IrisEmotionalMatrix.Instance != null &&
                                 //IrisEmotionalMatrix.Instance.currentMood == IrisEmotionalMatrix.IrisMood.Hysterical);


        // CORE LOGIC RULES:
        // 1. If she is in a warm room, she is safe (unless Iris causes a panic flash-freeze)
        // 2. If she is in a cold room, she stays warm ONLY if her lantern is burning tallow
        //if (!isIrisHysterical)
        {
            //if (!inColdZone) return; // Warm rooms completely protect Voss from ambient cold
            //if (isLanternLit) return; // Her light holds off the standard cold zone freeze loop
        }


        // Calculate whole number freeze drain speed per second
        float freezePointsPerSecond = 0.5f; // Standard dark freeze (1 point every 2 seconds)
       
        //if (isIrisHysterical) freezePointsPerSecond = 2.0f; // Rapid freezing panic
        //else if (!isLanternLit && inColdZone) freezePointsPerSecond = 1.0f; // Pitch black inside cold zones


        // Add energy down to the integer step converter
        freezeAccumulator += Time.deltaTime * freezePointsPerSecond;
       
        while (freezeAccumulator >= 1.0f)
        {
            currentTemperature = Mathf.Clamp(currentTemperature - 1, 0, maxTemperature);
            freezeAccumulator -= 1.0f;
            Debug.Log($"<color=cyan>[Thermal Loss]</color> Voss temperature dropped to standard integer: {currentTemperature}°C");
        }
    }


    private void ProcessProximityThawing()
    {
        freezeAccumulator = 0f; // Clear out frost registers
       
        if (currentTemperature >= maxTemperature)
        {
            thawAccumulator = 0f;
            return;
        }


        // --- REAL FIRE PROXIMITY CALCULATIONS ---
        float distance = Vector3.Distance(transform.position, activeNearbyHeatSource.transform.position);
       
        // Normalize distance into a 0 to 1 value (1 = standing inside the fire, 0 = at the very edge of the trigger)
        float proximityFactor = 1f - Mathf.Clamp01(distance / activeNearbyHeatSource.heatRadius);


        // Apply proximity curve to calculate dynamic heat gain per second
        float heatEnergyGain = (float)activeNearbyHeatSource.maxWarmthGeneration * proximityFactor;


        // Feed the decimal float into the integer accumulator engine
        thawAccumulator += Time.deltaTime * heatEnergyGain;


        while (thawAccumulator >= 1.0f)
        {
            currentTemperature = Mathf.Clamp(currentTemperature + 1, 0, maxTemperature);
            thawAccumulator -= 1.0f;
            Debug.Log($"<color=orange>[Thermal Absorption]</color> Voss approaches fire. Proximity: {(proximityFactor * 100f):F0}%. Temp: {currentTemperature}°C");
        }
    }


    private void EvaluateTemperaturePenalties()
    {
        if (player == null) return;


        isFreezing = (currentTemperature <= 30);


        if (currentTemperature <= 0)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= 2.0f)
            {
                player.TakeDamage(5);
                damageTimer = 0f;
            }
            player.walkSpeed = 2.0f;
            //player.staminaRegenRate = 0;
            return;
        }


        damageTimer = 0f;


        if (currentTemperature > 0 && currentTemperature <= 30)
        {
            player.walkSpeed = 2.4f;
            //player.staminaRegenRate = 0;
        }
        else if (currentTemperature > 30 && currentTemperature <= 69)
        {
            player.walkSpeed = 3.5f;
            //player.staminaRegenRate = 6;
        }
        else
        {
            player.walkSpeed = 3.5f;
            //player.staminaRegenRate = 12;
        }
    }


    private void UpdateFrostVisuals()
    {
        if (frostOverlayImage == null) return;
        float currentLossPercentage = 1f - ((float)currentTemperature / (float)maxTemperature);
        Color targetColor = frostOverlayImage.color;
        targetColor.a = currentLossPercentage;
        frostOverlayImage.color = targetColor;
    }


    public void InstantFlashFreeze(int penaltyAmount)
    {
        currentTemperature = Mathf.Clamp(currentTemperature - penaltyAmount, 0, maxTemperature);
        UpdateFrostVisuals();
    }
}
