using UnityEngine;


public class S_ClimateZone : MonoBehaviour
{
    [Header("Room Climate Properties")]
    [Tooltip("If true, this room is heated/insulated and keeps Voss warm. If false, it is a freezing environment.")]
    public bool isWarmRoom = false;


    private void OnTriggerEnter(Collider other)
    {
        S_TemperatureSystem temp = other.GetComponent<S_TemperatureSystem>();
        if (temp != null)
        {
            temp.RegisterCurrentRoom(this);
        }
    }


    private void OnTriggerExit(Collider other)
    {
        S_TemperatureSystem temp = other.GetComponent<S_TemperatureSystem>();
        if (temp != null)
        {
            // Clear the room reference only if she is actually leaving this specific room
            temp.UnregisterCurrentRoom(this);
        }
    }
}

