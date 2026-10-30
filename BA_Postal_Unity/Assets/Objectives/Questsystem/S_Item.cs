using TMPro;
using UnityEngine;

public class S_Item : MonoBehaviour
{
    public int ID;
    public string Name;
    public int quantity = 1;

    private TMP_Text quantityText;

    private void Awake()
    {
        quantityText = GetComponentInChildren<TMP_Text>(true);
        UpdateQuantityDisplay();
    }

    public void AddToStack(int amount = 1)
    {
        quantity += amount;
        UpdateQuantityDisplay();
    }

    public int RemoveFromStack(int amount = 1)
    {
        int removed = Mathf.Min(amount, quantity);
        quantity -= removed;
        UpdateQuantityDisplay();
        return removed;
    }

    public GameObject CloneItem(int newQuantity)
    {
        GameObject clone = Instantiate(gameObject);
        S_Item cloneItem = clone.GetComponent<S_Item>();

        if (cloneItem != null)
        {
            cloneItem.quantity = newQuantity;
            cloneItem.UpdateQuantityDisplay();
        }

        return clone;
    }

    public void UpdateQuantityDisplay()
    {
        if (quantityText != null)
        {
            quantityText.text = quantity > 1 ? quantity.ToString() : "";
        }
    }

    [Header("Inventory UI")]
    public GameObject inventoryPrefab;
}