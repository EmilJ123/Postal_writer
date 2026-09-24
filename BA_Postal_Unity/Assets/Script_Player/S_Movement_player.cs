using System;
using UnityEngine;
using UnityEngine. InputSystem;
using System.Collections.Generic;
public class S_Movement_player : MonoBehaviour
{
    public InputActionAsset actionAsset;
    public CharacterController playerCharacterController;
    public InputAction playerMoveAction;
    public InputAction playerJumpAction;

    [SerializeField] private Transform playerCamera;
    private Vector2 _playerMoveAmount;
    
    private float playerwalkSpeed = 5.0f;
    private float playerRotatedampening = 0.1f;
    private float turnsmoothingVelocity;
    
    private float verticalVelocity = 0f;
        private float gravity = 9.8f;
        private float jumpHeight = 5f;
        
    private void OnEnable()
    {
        InputAction.FindActionMap("Player").Enable();
        
    }

    private void OnDisable()
    {
        InputAction.FindActionMap("Player").Disable();
    }

    private void Awake()
    {
        playerMoveAction = InputSystem.actions.FindAction("Player");
        playerJumpAction = InputSystem.actions.FindAction("Player");
    }

    private void Update()
    {
        _playerMoveAmount = playerMoveAction.ReadValue<Vector2>();
        playerMoveAndRotate();
        //Jump();
    }

    private void playerMoveAndRotate()
    {
        Vector3 playerDirection = new Vector3(_playerMoveAmount.x, 0, _playerMoveAmount.y).normalized;
        Vector3 verticalMove = new Vector3(0f, verticalVelocity, 0f) * Time.deltaTime;

        if (playerDirection.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(playerDirection.x, playerDirection.z) * Mathf.Rad2Deg +  playerCamera.eulerAngles.y;
            float smoothTargetAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle , ref turnsmoothingVelocity, playerRotatedampening);
            
            transform.rotation = Quaternion.Euler(0f, smoothTargetAngle, 0f);

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            playerCharacterController.Move(moveDirection.normalized * (playerwalkSpeed * Time.deltaTime) + verticalMove);
        }
        else
        {
            playerCharacterController.Move(verticalMove);
        }
    }

   // private Jump()
   // {
        //if (playerCharacterController.isGrounded)
       // {
           // verticalVelocity = -1f;
            //if (playerJumpAction.WasPressedThisFrame())
           // {
               // verticalVelocity = jumpHeight;
           // }
      //  }
      //  else
       // {
           // verticalVelocity += gravity * Time.deltaTime;
       // }
   // }
        

}
