using UnityEngine;
using UnityEngine. InputSystem;

public class S_Player_movement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.8f;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private bool shouldFaceMoveDirection = false;
    
    private CharacterController controller;
    private Vector3 moveInput;
    private Vector3 velocity;
    

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        Debug.Log($"move Input {moveInput}");
    }

    public void Jump(InputAction.CallbackContext context)
    {
        Debug.Log($"Jumping {context.performed} - Is Grounded: {controller.isGrounded}");
        if (context.performed && controller.isGrounded)
        {
            Debug.Log("we're supposed to jump");
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    void Update()
    {
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        
        forward.y = 0;
        right.y = 0;
        
        forward.Normalize();
        right.Normalize();
        
        Vector3 moveDirection = forward * moveInput.y + right * moveInput.x;
        controller.Move(moveDirection * (speed * Time.deltaTime));

        if (shouldFaceMoveDirection && moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion toRotation =  Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, 10f * Time.deltaTime);
        }
        
        //Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        //controller.Move(moveDirection * (speed * Time.deltaTime));
        
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        
    }
    
}
