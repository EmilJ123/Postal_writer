using UnityEngine;

public class S_UnlockCursorWhileHoldingButton : MonoBehaviour
{
        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                UnlockCursor();
            }
            else
            {
                LockCursor();
            }
        }

        private void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
