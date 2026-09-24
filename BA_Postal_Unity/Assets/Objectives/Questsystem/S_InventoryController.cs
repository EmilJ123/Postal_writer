using UnityEngine;

public class S_InventoryController : MonoBehaviour
{
    public GameObject inventoryPanel;
    public GameObject slotPrefab;
   public int slotCount; // 
   public GameObject[] itemPrefabs;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < slotCount; i++)
        {
            S_Slot slot = Instantiate(slotPrefab, inventoryPanel.transform).GetComponent<S_Slot>();
            if(i < itemPrefabs.Length)
            {
                GameObject item = Instantiate(itemPrefabs[i], slot.transform);
                item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero; //Puts the item in the middle of the slot
                slot.currentItem = item;
            }
        }
    }
}
