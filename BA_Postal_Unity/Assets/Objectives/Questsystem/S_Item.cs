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
        quantityText = GetComponentInChildren<TMP_Text>();
    }

    public void UpdateQuantityDisplay()
    {
        quantityText.text = quantity > 1 ? quantity.ToString() : "";
    }

    [Header("Inventory UI")]
    public GameObject inventoryPrefab;
}
