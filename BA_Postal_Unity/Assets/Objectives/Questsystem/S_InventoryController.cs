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

    public List<S_InventorySaveData> GetInventoryItems()
    {
        List<S_InventorySaveData> invData = new List<S_InventorySaveData>();

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            S_Slot slot = slotTransform.GetComponent<S_Slot>();

            if (slot == null || slot.currentItem == null)
                continue;

            S_Item item = slot.currentItem.GetComponent<S_Item>();

            if (item != null)
            {
                invData.Add(
                    new S_InventorySaveData
                    {
                        itemID = item.ID,
                        slotIndex = slotTransform.GetSiblingIndex()
                    }
                );
            }
        }

        return invData;
    }

    public void SetInventoryItems(List<S_InventorySaveData> inventorySaveData)
    {
        foreach (Transform child in inventoryPanel.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < slotCount; i++)
        {
            Instantiate(slotPrefab, inventoryPanel.transform);
        }

        foreach (S_InventorySaveData data in inventorySaveData)
        {
            if (data.slotIndex >= slotCount)
                continue;

            S_Slot slot = inventoryPanel.transform.GetChild(data.slotIndex).GetComponent<S_Slot>();
            if (slot == null)
            continue;

            GameObject itemPrefab = itemDictionary.GetItemPrefab(data.itemID);
            if (itemPrefab == null) continue;

            S_Item worldItem = itemPrefab.GetComponent<S_Item>();
            if (worldItem == null || worldItem.inventoryPrefab == null)
            {
                continue;
            }

            GameObject inventoryItem = Instantiate(worldItem.inventoryPrefab, slot.transform);
            RectTransform rectTransform = inventoryItem.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = Vector2.zero;
            }
            slot.currentItem = inventoryItem;
        }
    }
}