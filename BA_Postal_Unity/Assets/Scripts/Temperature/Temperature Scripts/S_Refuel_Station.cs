using UnityEngine;

public class S_Refuel_Station : MonoBehaviour
{
    [Header("Refuel Settings")]
    [Tooltip("How much whole-number fuel this station gives per interaction.")]
    public int fuelRestoreAmount = 100;

    [Tooltip("Time in seconds it takes for the tallow vat to regenerate fuel.")]
    public float refillCooldown = 1f;


    [Header("Current State")]
    public bool isReady = true;
    public float currentCooldownTimer = 0f;


    private MeshRenderer meshRenderer;


    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        UpdateVisualState();
    }


    private void Update()
    {
        if (!isReady)
        {
            currentCooldownTimer += Time.deltaTime;


            if (currentCooldownTimer >= refillCooldown)
            {
                isReady = true;
                currentCooldownTimer = 0f;
                UpdateVisualState();
                Debug.Log($"<color=yellow>[Station Ready]</color> The tallow fat has rendered completely at {gameObject.name}. Ready for collection.");
            }
        }
    }


    public void TryRefillLantern(S_Player_movement player)
    {
        if (!isReady)
        {
            int secondsLeft = Mathf.CeilToInt(refillCooldown - currentCooldownTimer);
            Debug.LogWarning($"The tallow boiling vat is cold. It needs approximately {secondsLeft} seconds to render more whale fat.");
            return;
        }


        S_LanternSystem lantern = player.GetComponent<S_LanternSystem>();
        if (lantern == null)
        {
            Debug.LogError("Voss doesn't have a LanternSystem component attached to her GameObject!");
            return;
        }


        if (lantern.currentFuel >= lantern.maxFuel)
        {
            Debug.Log("Voss's lantern is already brimming with tallow. No need to waste resources.");
            return;
        }


        // Execute whole number transfer
        lantern.AddTallow(fuelRestoreAmount);
        isReady = false;
        currentCooldownTimer = 0f;
        UpdateVisualState();


        Debug.Log($"<color=yellow>[Refuel Success]</color> Voss scoops fresh, warm tallow into her lantern. Fuel restored!");
    }


    private void UpdateVisualState()
    {
        if (meshRenderer != null)
        {
            // Yellow = Full of warm tallow fat | Gray = Empty/Cold
            meshRenderer.material.color = isReady ? Color.yellow : Color.gray;
        }
    }
}