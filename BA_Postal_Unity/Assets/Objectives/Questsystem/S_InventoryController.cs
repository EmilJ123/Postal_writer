using System;
using System.Collections.Generic;
using UnityEngine;

public class S_InventoryController : MonoBehaviour
{
    private S_ItemDictionary itemDictionary;

    public GameObject inventoryPanel;
    public GameObject slotPrefab;

    public int slotCount;
    public GameObject[] itemPrefabs;
    Dictionary<int, int> itemsCountCache = new();
    public event Action OnInventoryChanged; //event to notify quest systems

    private void Start()
    {
        itemDictionary = FindAnyObjectByType<S_ItemDictionary>();
        RebuildItemCounts();
    }

    public void RebuildItemCounts()
    {
        itemsCountCache.Clear();

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            S_Slot slot = slotTransform.GetComponent<S_Slot>();
            if(slot.currentItem != null)
            {
                S_Item item = slot.currentItem.GetComponent<S_Item>();
                if(item != null)
                {
                    itemsCountCache[item.ID] = itemsCountCache.GetValueOrDefault(item.ID, 0) + item.quantity;
                }
            }
        }

        OnInventoryChanged.Invoke();
    }

    public Dictionary<int, int> GetItemCounts() => itemsCountCache;

/// ///////////////////////////////////////////////////////////
/// Stack Items
///////////////////////////////////////////////////////////////
    
    public bool AddItem(GameObject itemPrefab)
    {
        S_Item itemToAdd = itemPrefab.GetComponent<S_Item>();
        if (itemToAdd == null) return false;

        //check if we have this item in inventory
        foreach(Transform slotTransform in inventoryPanel.transform)
        {
            S_Slot slot = slotTransform.GetComponent<S_Slot>();
            if (slot != null && slot.currentItem != null)
            {
                S_Item slotItem = slot.currentItem.GetComponent<S_Item>();
                if (slotItem != null && slotItem.ID == itemToAdd.ID)
                {
                    //Same item, stack them
                    slotItem.AddToStack();
                    RebuildItemCounts();
                    return true;
                }
            }      
        }

        //Look for empty slot
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            S_Slot slot = slotTransform.GetComponent<S_Slot>();
            if (slot != null && slot.currentItem == null)
            {
                GameObject newItem = Instantiate(itemPrefab, slotTransform);
                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                RebuildItemCounts();
                return true;
            }
        }

        Debug.Log("Inventory is full!");
        return false;
    }

    ///////////////////////////////////////////////////////////
    /// Save Inevntory Data
    ///////////////////////////////////////////////////////////
   
    public List<S_InventorySaveData> GetInventoryItems()
    {
        List<S_InventorySaveData> invData = new List<S_InventorySaveData>();
        foreach (Transform slotTranform in inventoryPanel.transform)
        {
            S_Slot slot = slotTranform.GetComponent<S_Slot>();
            if (slot.currentItem != null)
            {
                S_Item item = slot.currentItem.GetComponent<S_Item>();
                invData.Add(new S_InventorySaveData
                {
                    itemID = item.ID,
                    slotIndex = slot.transform.GetSiblingIndex(),
                    quantity = item.quantity
                });
            }
        }

        return invData;
    }

public void SetInventoryItems(List<S_InventorySaveData> inventorySaveData)
{
    foreach (Transform slotTransform in inventoryPanel.transform)
    {
        Destroy(slotTransform.gameObject);
    }

    for (int i = 0; i < slotCount; i++)
    {
        Instantiate(slotPrefab, inventoryPanel.transform);
    }

    foreach (S_InventorySaveData data in inventorySaveData)
    {
        if (data.slotIndex < 0 || data.slotIndex >= slotCount)
            continue;

        S_Slot slot = inventoryPanel.transform
            .GetChild(data.slotIndex)
            .GetComponent<S_Slot>();

        if (slot == null)
            continue;

        GameObject itemPrefab = itemDictionary.GetItemPrefab(data.itemID);

        if (itemPrefab == null)
        {
            Debug.LogWarning($"No item prefab found for ID {data.itemID}");
            continue;
        }

        GameObject item = Instantiate(itemPrefab, slot.transform);

        RectTransform rectTransform = item.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = Vector2.zero;
        }

        S_Item itemComponent = item.GetComponent<S_Item>();

        if (itemComponent != null)
        {
            itemComponent.quantity = data.quantity;
            itemComponent.UpdateQuantityDisplay();
            slot.currentItem = item;
        }
        else
        {
            Destroy(item);
        }
    }

    RebuildItemCounts();
}
}