using System.Collections.Generic;
using UnityEngine;

public class S_ItemDictionary : MonoBehaviour
{
    public List<S_Item> itemPrefabs;
    private Dictionary<int, GameObject> itemDictionary;

    private void Awake()
    {
        itemDictionary = new Dictionary<int, GameObject>();

        if (itemPrefabs == null)
        {
            Debug.LogError("Item prefabs list is not assigned.", this);
            return;
        }

        for (int i = 0; i < itemPrefabs.Count; i++)
        {
            S_Item item = itemPrefabs[i];

            if (item == null)
            {
                Debug.LogWarning($"Missing item prefab at index {i}.", this);
                continue;
            }

            item.ID = i + 1;
            itemDictionary[item.ID] = item.gameObject;
        }
    }

    public GameObject GetItemPrefab(int itemID)
    {
        if (itemDictionary == null)
        {
            Debug.LogError("Item dictionary has not been initialized.", this);
            return null;
        }

        if (itemDictionary.TryGetValue(itemID, out GameObject itemPrefab)
            && itemPrefab != null)
        {
            return itemPrefab;
        }

        Debug.LogWarning($"Item with ID {itemID} not found in the dictionary.", this);
        return null;
    }
}