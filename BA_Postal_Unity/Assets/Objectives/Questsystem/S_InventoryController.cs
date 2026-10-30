using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TMPro;

public class S_InventoryController : MonoBehaviour
{
    private S_ItemDictionary itemDictionary;

    public GameObject inventoryPanel;
    public GameObject slotPrefab;

    public int slotCount;
    public GameObject[] itemPrefabs;
    Dictionary<int, int> itemsCountCache = new();
    public event Action OnInventoryChanged; //event to notify quest systems
    public static S_InventoryController Instance { get; private set; }

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
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

        OnInventoryChanged?.Invoke();
    }

    public Dictionary<int, int> GetItemCounts() => itemsCountCache;

    public bool AddItem(GameObject itemPrefab)
    {
        if (itemPrefab == null || inventoryPanel == null)
            return false;

        S_Item itemToAdd = itemPrefab.GetComponent<S_Item>();
        if (itemToAdd == null)
            return false;

        // Try to stack with an existing item
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            S_Slot slot = slotTransform.GetComponent<S_Slot>();

            if (slot == null || slot.currentItem == null)
                continue;

            S_Item slotItem = slot.currentItem.GetComponent<S_Item>();

            if (slotItem != null && slotItem.ID == itemToAdd.ID)
            {
                slotItem.AddToStack();
                RebuildItemCounts();
                return true;
            }
        }

        // Find an empty slot
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            S_Slot slot = slotTransform.GetComponent<S_Slot>();

            if (slot == null || slot.currentItem != null)
                continue;

            GameObject newItem = Instantiate(itemPrefab, slotTransform);
            newItem.transform.localPosition = Vector3.zero;
            newItem.transform.localRotation = Quaternion.identity;
            newItem.transform.localScale = Vector3.one;

            // Register the item with the slot
            slot.currentItem = newItem;

            RebuildItemCounts();
            return true;
        }

        Debug.LogWarning("Inventory is full!");
        return false;
    }

    public List<S_InventorySaveData> GetInventoryItems()
    {
        List<S_InventorySaveData> invData = new List<S_InventorySaveData>();

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            S_Slot slot = slotTransform.GetComponent<S_Slot>();
            if (slot == null || slot.currentItem == null)
            {
                S_Item item = slot.currentItem.GetComponent<S_Item>();
                invData.Add(new S_InventorySaveData
                {
                    itemID = item.ID,
                    slotIndex = slotTransform.GetSiblingIndex(),
                    quantity = item.quantity
                });
            }

        }

        return invData;
    }

    public void SetInventoryItems(List<S_InventorySaveData> inventorySaveData)
    {
        //Clear inventory panel - avoid duplicates
        foreach (Transform child in inventoryPanel.transform)
        {
            Destroy(child.gameObject);
        }

        //Create new slots
        for (int i = 0; i < slotCount; i++)
        {
            Instantiate(slotPrefab, inventoryPanel.transform);
        }

        foreach (S_InventorySaveData data in inventorySaveData)
        {
            S_Slot slot = inventoryPanel.transform.GetChild(data.slotIndex).GetComponent<S_Slot>();
            GameObject itemPrefab = itemDictionary.GetItemPrefab(data.itemID);
            if (itemPrefab != null)
            {
                GameObject item = Instantiate(itemPrefab, slot.transform);

                item.transform.localPosition = Vector3.zero;
                item.transform.localRotation = Quaternion.identity;

                S_Item itemComponent = item.GetComponent<S_Item>();
                if(itemComponent != null && data.quantity > 1)
                {
                    itemComponent.quantity = data.quantity;
                    itemComponent.UpdateQuantityDisplay();
                }

                slot.currentItem = item;
            }
        }

        RebuildItemCounts();
    }
}