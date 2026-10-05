using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_PlayerItemCollector : MonoBehaviour
{
    [SerializeField] private S_InventoryController inventoryController;
    
    private GameObject currentItemInRange;

    void Start()
    {
        if (inventoryController == null)
        {
            inventoryController = GameObject.FindAnyObjectByType<S_InventoryController>();
            if (inventoryController == null)
            {
                Debug.LogError($"{gameObject.name} is missing an S_InventoryController!");
            }
        }
    }

    void Update()
    {
        if (currentItemInRange != null && Input.GetKeyDown(KeyCode.F))
        {
            TryPickUpItem();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Item"))
        {
            currentItemInRange = other.gameObject;
            
            Debug.Log("Press F to pick up" + other.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Item"))
        {
            if (currentItemInRange == other.gameObject)
            {
                currentItemInRange = null;
            }
        }
    }

    private void TryPickUpItem()
    {
        if (currentItemInRange.TryGetComponent<S_Item>(out S_Item item))
        {
            if (inventoryController != null && inventoryController.AddItem(item.gameObject))
            {
                Destroy(currentItemInRange);
                currentItemInRange = null;
            }
        }
    }
}

