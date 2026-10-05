using UnityEngine;
using Unity.Collections;
using System.Collections.Generic;
    
public class S_MapUIManager : MonoBehaviour
{
    
    [Header("References")]
    [SerializeField] private GameObject mapPanel;
    [SerializeField] private S_MapGridController gridController;
   
    
    [Header("Controls")]
    [SerializeField] private KeyCode toggleKey = KeyCode.X;
    
    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleMap();

            Cursor.visible = !Cursor.visible;

            if (Cursor.visible)
            {
                Cursor.lockState = CursorLockMode.Locked;
                //Cursor.visible = false;
                
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                
            }
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

