using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class S_Temperature : MonoBehaviour
{
    [Header("Whole Number Vitals")]
    public int maxHealth = 100;
    public int currentHealth = 100;
    public bool isDead = false;
    
    [SerializeField] public Slider healthSlider;
    public Transform respawnPoint;
    void Start()
    {
        currentHealth = maxHealth;
        //currentStamina = maxStamina;
        if (healthSlider != null)
            healthSlider.value = maxHealth;
    }
    public void TakeDamage(int amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        Debug.LogError($"<color=red>[Damage Taken]</color> HP: {currentHealth}/{maxHealth}");
        UpdateHealthUI();


        if (currentHealth <= 0)
        {
            Die();
            
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0,maxHealth);
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (healthSlider != null)
            healthSlider.value = currentHealth;
       
    }
    private void Die()
    {
        isDead = true;
        Debug.LogError("GAME OVER: you froze in the dark");
        PlayerInput input = GetComponent<PlayerInput>();
        if (input != null)
        {
            input.DeactivateInput();
            input.enabled = false;
        }
        
        Invoke(nameof(Respawn), 2f);
    }
    private Transform FindNearestHeatSource()
    {
        S_HeatSource[] sources = FindObjectsByType<S_HeatSource>(FindObjectsInactive.Exclude);

        float closestDist = Mathf.Infinity;
        Transform bestSource = null;

        foreach (var src in sources)
        {
            if (!src.isActive) continue; // only safe heat sources

            float dist = Vector3.Distance(transform.position, src.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                bestSource = src.transform;
            }
        }

        return bestSource;
    }
    private void Respawn()
    {
        isDead = false;
        currentHealth = maxHealth;

        // Reset temperature
        S_TemperatureSystem tempSystem = GetComponent<S_TemperatureSystem>();
        if (tempSystem != null)
            tempSystem.currentTemperature = tempSystem.maxTemperature;

        // Find nearest heat source
        Transform safeSpot = FindNearestHeatSource();

        CharacterController controller = GetComponent<CharacterController>();
        controller.enabled = false;

        if (safeSpot != null)
        {
            // Spawn slightly beside the heat source
            Vector3 offset = safeSpot.right * 2f;
            transform.position = safeSpot.position + offset;
        }
        else
        {
            // fallback to default respawn point
            transform.position = respawnPoint.position;
        }

        controller.enabled = true;

        // Re-enable input
        PlayerInput input = GetComponent<PlayerInput>();
        input.enabled = true;
        input.ActivateInput();

        Debug.Log("<color=green>Player respawned at nearest safe heat source!</color>");
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.color = Color.red;
        //Gizmos.DrawWireSphere(transform.position, shoveRadius);
    }
}
