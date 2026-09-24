using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerMovement : MonoBehaviour
{
    
    private Vector3 moveInput;

    public float moveSpeed;
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector3>();
    }

    private void Update()
    {
        transform.Translate(moveInput * Time.deltaTime * moveSpeed);
    }
}
