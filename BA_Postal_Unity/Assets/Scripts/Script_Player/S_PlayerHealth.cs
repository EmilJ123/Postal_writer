using UnityEngine;

public class S_PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    private Transform respawnPoint;
    private CharacterController controller;

    private void Start()
    {
        currentHealth = maxHealth;
        controller = GetComponent<CharacterController>();
    }

    public void takeHealthDamage(int amount)
    {
        currentHealth -= amount;

        if (transform)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
    }

    private void Die()
    {
        Debug.Log("<color=red>Player died from freezing!</color>");

        bool wasEnabled = controller.enabled;
        controller.enabled = false;
        
        transform.position = respawnPoint.position;
        
        currentHealth = maxHealth;

        controller.enabled = wasEnabled;
    }

}
