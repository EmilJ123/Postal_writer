using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class S_WeatherZone : MonoBehaviour
{
    public float snowAmount;
    public float windStrength;
    public float fogDensity;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(gameObject.name + " entered");

        if(other.CompareTag("Player"))
        {
            S_WeatherManager.Instance.SetWeather(
                snowAmount,
                windStrength,
                fogDensity
            );
        }
    }
}
