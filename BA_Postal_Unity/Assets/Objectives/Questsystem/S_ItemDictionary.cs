using System.Collections.Generic;
using UnityEngine;

public class S_ItemDictionary : MonoBehaviour
{
    public List<S_Item> itemPrefabs;
    private Dictionary<int, GameObject> itemDictionary;
    
   private void Awake()
    {
        itemDictionary = new Dictionary<int, GameObject>();

        for(int i = 0; i < itemPrefabs.Count; i++)
        {
            if(itemPrefabs[i] != null)
            {
                itemPrefabs[i].ID = i + 1; 
            }
            
        }

        foreach(S_Item item in itemPrefabs)
        {
            itemDictionary[item.ID] = item.gameObject;
        }

    }

    public GameObject GetItemPrefab(int itemID)
    {
        itemDictionary.TryGetValue(itemID, out GameObject itemPrefab);
        if(itemPrefab == null)
        {
            Debug.LogWarning($"Item with ID {itemID} not found in the dictionary.");
        }
        return itemPrefab;
    }
   
}
