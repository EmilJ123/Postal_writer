using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class S_ColorButton : MonoBehaviour
{
    [SerializeField] private Color fillPenColor = Color.yellow;
    [SerializeField] private Color outlineColor = new Color(1f, 0.5f, 0f); // Darker orange/brown
    [SerializeField] private S_MapGridController gridController;

    private void Awake()
    {
        if (gridController == null)
            gridController = FindAnyObjectByType<S_MapGridController>();

        GetComponent<Button>().onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        if (gridController != null)
        {
            gridController.SetPenColorWithOutline(fillPenColor, outlineColor);
        }
    }
}