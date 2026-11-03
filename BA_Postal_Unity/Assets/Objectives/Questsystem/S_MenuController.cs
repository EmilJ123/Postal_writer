using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class S_MenuController : MonoBehaviour
{
    [SerializeField] private GameObject menuCanvas;
    [SerializeField] private PlayerInput playerInput;

    private bool menuOpen;

    private void Start()
    {
        menuCanvas.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            ToggleMenu();
        }
    }

    private void ToggleMenu()
    {
        menuOpen = !menuOpen;

        menuCanvas.SetActive(menuOpen);

        if (menuOpen)
        {
            // Switch to UI controls
            playerInput.SwitchCurrentActionMap("UI");

            // Free cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // Switch back to gameplay controls
            playerInput.SwitchCurrentActionMap("Player");

            // Lock cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}