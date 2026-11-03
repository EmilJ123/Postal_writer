using UnityEngine;
using UnityEngine.UI;

public class S_BrushSizeUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private S_MapGridController gridController;
    [SerializeField] private Slider brushSizeSlider;

    [Header("Slider Settings")]
    [SerializeField] private float minRadius = 1f;
    [SerializeField] private float maxRadius = 20f;

    private void Awake()
    {
        if (gridController == null)
            gridController = FindAnyObjectByType<S_MapGridController>();

        if (brushSizeSlider != null)
        {
            brushSizeSlider.minValue = minRadius;
            brushSizeSlider.maxValue = maxRadius;
            brushSizeSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }
    }

    // Call from UI Slider -> On Value Changed
    public void OnSliderValueChanged(float value)
    {
        if (gridController != null)
        {
            gridController.SetPenRadius(value);
        }
    }

    // Call from Preset Buttons:
    // Small Button  -> OnClick -> Call OnPresetSmall()
    // Medium Button -> OnClick -> Call OnPresetMedium()
    // Large Button  -> OnClick -> Call OnPresetLarge()
    public void OnPresetSmall() => SetPreset(0, 3f);
    public void OnPresetMedium() => SetPreset(1, 7f);
    public void OnPresetLarge() => SetPreset(2, 12f);

    private void SetPreset(int index, float radiusValue)
    {
        if (gridController != null)
        {
            gridController.SetPresetSize(index);
        }

        // Sync slider position with selected preset
        if (brushSizeSlider != null)
        {
            brushSizeSlider.SetValueWithoutNotify(radiusValue);
        }
    }
}