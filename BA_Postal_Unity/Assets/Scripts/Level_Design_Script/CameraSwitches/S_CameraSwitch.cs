using UnityEngine;
using Unity.Cinemachine;

public class S_CameraSwitch : MonoBehaviour
{
    [SerializeField] private CinemachineCamera[] _cameraSettings;
    [SerializeField] private int _whichCamera;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (_cameraSettings == null || _cameraSettings.Length == 0)
        {
            Debug.LogWarning($"No cameras assigned on {gameObject.name}.");
            return;
        }

        if (_whichCamera < 0 || _whichCamera >= _cameraSettings.Length)
        {
            Debug.LogWarning($"_whichCamera ({_whichCamera}) is out of range on {gameObject.name}. Valid range: 0 to {_cameraSettings.Length - 1}.");
            return;
        }

        foreach (var c in _cameraSettings)
        {
            if (c != null)
                c.Priority = 10; // Resets each camera's priority value back to 10
        }

        _cameraSettings[_whichCamera].Priority = 15;
    }
}