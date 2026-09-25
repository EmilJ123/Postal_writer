using UnityEngine;


[RequireComponent(typeof(SphereCollider))]
public class S_HeatSource : MonoBehaviour
{
    [Header("Dynamic Thermal Settings")]
    public bool isActive = true;
   
    [Tooltip("Maximum heat points restored per second when standing directly against the object.")]
    public int maxWarmthGeneration = 12;


    [Tooltip("This should match the radius of your Sphere Collider trigger for accurate distance falloff calculations.")]
    public float heatRadius = 4f;


    private SphereCollider sphereCollider;


    private void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider != null)
        {
            sphereCollider.isTrigger = true;
            heatRadius = sphereCollider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        S_TemperatureSystem temp = other.GetComponent<S_TemperatureSystem>();
        if (temp != null && isActive)
        {
            temp.RegisterHeatSource(this);
        }
    }


    private void OnTriggerExit(Collider other)
    {
        S_TemperatureSystem temp = other.GetComponent<S_TemperatureSystem>();
        if (temp != null)
        {
            temp.UnregisterHeatSource(this);
        }
    }
}
