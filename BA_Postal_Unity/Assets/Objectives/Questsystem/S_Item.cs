
using TMPro;
using UnityEngine;

public class S_Item : MonoBehaviour
{
    [Header("Item Data")]
    public int ID;
    public string Name;
    public int quantity = 1;

    [Header("Inventory UI")]
    public GameObject inventoryPrefab;

    private TMP_Text quantityText;

    private void Awake()
    {
        FindQuantityText();
        UpdateQuantityDisplay();
    }

    private void FindQuantityText()
    {
        quantityText = GetComponentInChildren<TMP_Text>(true);
    }

    public void AddToStack(int amount = 1)
    {
        if (amount <= 0)
            return;

        quantity += amount;
        UpdateQuantityDisplay();
    }

    public int RemoveFromStack(int amount = 1)
    {
        if (amount <= 0)
            return 0;

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
            cloneItem.quantity = Mathf.Max(1, newQuantity);
            cloneItem.UpdateQuantityDisplay();
        }

        return clone;
    }

    public void UpdateQuantityDisplay()
    {
        if (quantityText == null)
            FindQuantityText();

        if (quantityText == null)
            return;

        quantityText.text = quantity > 1
            ? quantity.ToString()
            : "";
    }
}
