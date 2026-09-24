using UnityEngine;


public class S_ClimateZone : MonoBehaviour
{
    [Header("Room Climate Properties")]
    [Tooltip("If true, this room is heated/insulated and keeps Voss warm. If false, it is a freezing environment.")]
    public bool isWarmRoom = false;


    private void OnTriggerEnter(Collider other)
    {
        TemperatureSystem temp = other.GetComponent<TemperatureSystem>();
        if (temp != null)
        {
            temp.RegisterCurrentRoom(this);
        }
    }


    private void OnTriggerExit(Collider other)
    {
        TemperatureSystem temp = other.GetComponent<TemperatureSystem>();
        if (temp != null)
        {
            // Clear the room reference only if she is actually leaving this specific room
            temp.UnregisterCurrentRoom(this);
        }
    }
}

