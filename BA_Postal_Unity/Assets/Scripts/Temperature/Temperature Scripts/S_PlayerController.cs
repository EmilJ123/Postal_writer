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
    

    void Start()
    {
        currentHealth = maxHealth;
        //currentStamina = maxStamina;
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
        Gizmos.color = Color.red;
        //Gizmos.DrawWireSphere(transform.position, shoveRadius);
    }
}
