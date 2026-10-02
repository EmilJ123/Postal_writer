using UnityEngine;

public class S_MapUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject mapPanel;
    [SerializeField] private S_MapGridController gridController;

    [Header("Controls")]
    [SerializeField] private KeyCode toggleKey = KeyCode.X;

    private void Update()
    {
        // Simple toggle check
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleMap();
        }
    }

    public void ToggleMap()
    {
        if (mapPanel != null)
        {
            bool currentState = mapPanel.activeSelf;
            mapPanel.SetActive(!currentState); // Flips between enabled and disabled
        }
        else
        {
            Debug.LogError("MapPanel is not assigned in MapUIManager!");
        }
    }

    // UI Button Callbacks
    public void SelectPenTool() => gridController?.SetTool(MapTool.Pen);
    public void SelectEraserTool() => gridController?.SetTool(MapTool.Eraser);
    public void ClearMap() => gridController?.ClearTexture();
}