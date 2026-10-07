using System;
using UnityEngine;

public class S_PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    public void Jump()
    {
        Debug.Log("Jump");
        rb.AddForce(new Vector3(0, 1, 0));
    }
}
