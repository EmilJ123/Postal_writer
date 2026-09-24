using UnityEngine;
using UnityEngine.EventSystems;

public class MapUIManager : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [Header("References")]
    [SerializeField] private GameObject mapPanel;
    [SerializeField] private MapGridController gridController;

    [Header("Controls")]
    [SerializeField] private KeyCode toggleKey = KeyCode.X;

    private bool isMapOpen = false;

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleMap();
        }
    }

    public void ToggleMap()
    {
        isMapOpen = !isMapOpen;
        mapPanel.SetActive(isMapOpen);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        gridController.HandlePointerInput(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        gridController.HandlePointerInput(eventData.position);
    }

    // UI Button Triggers
    public void SelectPenTool() => gridController.SetTool(MapTool.Pen);
    public void SelectEraserTool() => gridController.SetTool(MapTool.Eraser);
    public void ClearMap() => gridController.ClearTexture();
}