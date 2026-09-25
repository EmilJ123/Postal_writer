using System.IO;
using UnityEngine;

public class S_SaveController : MonoBehaviour
{
    private string saveLocation;
    private S_InventoryController inventoryController;

    [System.Obsolete]
    void Start()
    {
 
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
        
        inventoryController = FindObjectOfType<S_InventoryController>();
        
   
        LoadGame();
    }

    [System.Obsolete]
    public void SaveGame()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player"); 
        if (player == null)
        {
            Debug.LogError("SaveGame feilet: Fant ingen GameObject med taggen 'Player'.");
            return;
        }

        Rigidbody rb = FindObjectOfType<Rigidbody>();

        S_SaveData saveData = new S_SaveData
        {
            playerPosition = player.transform.position,
            mapBoundary = rb != null ? rb.gameObject.name : "", 
            inventorySaveData = inventoryController != null ? inventoryController.GetInventoryItems() : null
        };

        // Skriver data til disk
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(saveLocation, json);
        Debug.Log($"Spill lagret til: {saveLocation}");
    }

    [System.Obsolete]
    public void LoadGame()
    {
        if (File.Exists(saveLocation))
        {
            string json = File.ReadAllText(saveLocation);
            S_SaveData saveData = JsonUtility.FromJson<S_SaveData>(json);

            // Gjenopprett spillerposisjon
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = saveData.playerPosition;
            }

            // Gjenopprett inventar
            if (inventoryController != null && saveData.inventorySaveData != null)
            {
                inventoryController.SetInventoryItems(saveData.inventorySaveData);
            }

            
            if (!string.IsNullOrEmpty(saveData.mapBoundary))
            {
                GameObject boundaryObj = GameObject.Find(saveData.mapBoundary);
                if (boundaryObj != null)
                {
                
                }
            }

            Debug.Log("Spill lastet inn suksessfullt.");
        }
        else
        {
            Debug.LogWarning("Ingen lagringsfil funnet. Genererer en ny startfil.");
            SaveGame();
        }
    }
}
