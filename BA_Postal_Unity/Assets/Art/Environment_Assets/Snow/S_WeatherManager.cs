using UnityEngine;
using UnityEngine.VFX;

public class S_WeatherManager : MonoBehaviour
{
    public static S_WeatherManager Instance;
    public VisualEffect snowVFX;

    [SerializeField] private float transitionSpeed = 5f;

    private float targetSnow;
    private float targetWind;
    private float targetFog;
    
    private float currentSnow;
    private float currentWind;
    private void Awake()
    {
        Instance = this;
        
        targetFog = RenderSettings.fogDensity;
        
    }

    private void Update()
    {
        RenderSettings.fogDensity = Mathf.Lerp(
            RenderSettings.fogDensity,
            targetFog,
            transitionSpeed * Time.deltaTime);
    }
    public void SetWeather(float snow, float wind, float fog)
    {
        //RenderSettings.fogDensity = fog;
        targetFog = fog;
    }
}
