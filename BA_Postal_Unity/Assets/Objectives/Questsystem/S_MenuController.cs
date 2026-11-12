using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class S_MenuController : MonoBehaviour
{
    
    public GameObject menuCanvas;
    public CinemachineInputAxisController cameraInput;
    public PlayerInput playerInput;
    private bool menuOpen;

    private void Start()
    {
        menuCanvas.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            menuOpen = !menuOpen;

            menuCanvas.SetActive(menuOpen);

            if (menuOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                cameraInput.enabled = false;
                
                playerInput.actions.FindActionMap("Gameplay").Disable();
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                cameraInput.enabled = true;
                
                playerInput.actions.FindActionMap("Gameplay").Enable();
            }
        }
    }
}