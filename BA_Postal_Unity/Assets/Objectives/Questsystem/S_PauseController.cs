using UnityEngine;

public class S_PauseController : MonoBehaviour
{
    public static bool IsGamePaused { get; private set; } = false;

    public static void SetPauseState(bool pause)
    {
        IsGamePaused = pause;
    }
}
