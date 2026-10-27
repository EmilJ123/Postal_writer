using System.Collections.Generic;
using UnityEngine;

public class S_PlayerItemCollector : MonoBehaviour
{
    [SerializeField] private S_InventoryController inventoryController;

    private readonly List<GameObject> itemsInRange = new List<GameObject>();

    private void Start()
    {
        if (inventoryController == null)
        {
            inventoryController = FindAnyObjectByType<S_InventoryController>();

            if (inventoryController == null)
            {
                Debug.LogError($"{gameObject.name} is missing an S_InventoryController!");
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            TryPickUpClosestItem();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Item"))
            return;

        if (!itemsInRange.Contains(other.gameObject))
        {
            itemsInRange.Add(other.gameObject);
            Debug.Log($"Press F to pick up {other.name}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Item"))
            return;

        itemsInRange.Remove(other.gameObject);
    }

    private void TryPickUpClosestItem()
    {
        GameObject closestItem = GetClosestItem();

        if (closestItem == null)
            return;

        if (!closestItem.TryGetComponent<S_Item>(out S_Item item))
        {
            Debug.LogWarning($"{closestItem.name} has the Item tag but no S_Item component.");
            return;
        }

        if (inventoryController == null)
            return;

        if (inventoryController.AddItem(item.gameObject))
        {
            itemsInRange.Remove(closestItem);
            Destroy(closestItem);
        }
    }

    private GameObject GetClosestItem()
    {
        GameObject closestItem = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject item in itemsInRange)
        {
            if (item == null)
                continue;

            float distance = Vector3.Distance(transform.position, item.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestItem = item;
            }
        }

        return closestItem;
    }
}