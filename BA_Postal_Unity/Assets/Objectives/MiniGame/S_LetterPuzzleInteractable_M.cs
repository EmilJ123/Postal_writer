using UnityEngine;

public class S_LetterPuzzleInteractable_M : MonoBehaviour
{
    [SerializeField] private S_PuzzleManager_M puzzleManager;

    public void OpenPuzzle()
    {
        puzzleManager.OpenPuzzle();
    }
}
