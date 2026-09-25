using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_InventoryController : MonoBehaviour
{
    private S_ItemDictionary itemDictionary;
    public GameObject inventoryPanel;
    public GameObject slotPrefab;
   public int slotCount; // 
   public GameObject[] itemPrefabs;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [System.Obsolete]
    void Start()
    {
        itemDictionary = FindObjectOfType<S_ItemDictionary>();

        //for (int i = 0; i < slotCount; i++)
        //{
            //S_Slot slot = Instantiate(slotPrefab, inventoryPanel.transform).GetComponent<S_Slot>();
            //if(i < itemPrefabs.Length)
            //{
              //  GameObject item = Instantiate(itemPrefabs[i], slot.transform);
                //item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero; //Puts the item in the middle of the slot
                //slot.currentItem = item;
            //}
        //}
    }

    public List<S_InventorySaveData> GetInventoryItems()
    {
        List<S_InventorySaveData> invData = new List<S_InventorySaveData>();
        foreach(Transform slotTransform in inventoryPanel.transform)
        {
            S_Slot slot = slotTransform.GetComponent<S_Slot>();
            if(slot.currentItem != null)
            {
                S_Item item = slot.currentItem.GetComponent<S_Item>();
                invData.Add(new S_InventorySaveData { itemID = item.ID, slotIndex = slotTransform.GetSiblingIndex() });
            }
        }
        return invData;
    }

    public void SetInventoryItems(List<S_InventorySaveData> inventorySaveData)
    {
        foreach(Transform child in inventoryPanel.transform)
        {
           Destroy(child.gameObject); 
        }

        for(int i = 0; i < slotCount; i++)
        {
            Instantiate(slotPrefab, inventoryPanel.transform);
        }

        foreach(S_InventorySaveData data in inventorySaveData)
        {
            if(data.slotIndex < slotCount)
            {
                S_Slot slot = inventoryPanel.transform.GetChild(data.slotIndex).GetComponent<S_Slot>();
                GameObject itemPrefab = itemDictionary.GetItemPrefab(data.itemID);
                if(itemPrefab != null)
                {
                    GameObject item = Instantiate(itemPrefab, slot.transform);
                    item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero; 
                    slot.currentItem = item;
                }
            
            }
        }

    }

}
