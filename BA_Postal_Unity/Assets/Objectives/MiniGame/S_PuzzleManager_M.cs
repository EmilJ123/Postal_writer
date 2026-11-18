using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
public class S_PuzzleManager_M : MonoBehaviour
{
    [SerializeField] private GameObject letterPuzzleUI;
    [SerializeField] private GameObject completedLetterUI;
    public CinemachineInputAxisController cameraInput;
    public PlayerInput playerInput;
    public string[] correctSentence =
    {
        "The",
        "cat",
        "Happy",
        "Runa",
        "Carmilla",
        "Winter",
        
    };
    public void ContinueConversation()
    {
        completedLetterUI.SetActive(false);
        
        //FindObjectsByType<S_Dialogue3>().ContinueDialogue();
    }
    
    private void EnterPuzzleMode()
    {
        Debug.Log("ENTERING PUZZLE MODE");

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        cameraInput.enabled = false;

        Debug.Log("Current Map: " + playerInput.currentActionMap.name);
        playerInput.actions.FindActionMap("UI").Disable();
        Debug.Log("New Map: " + playerInput.currentActionMap.name);
        
    }

    private void ExitPuzzleMode()
    {
        if (cameraInput != null)
            cameraInput.enabled = true;

        if (playerInput != null)
            playerInput.actions.FindActionMap("Player").Enable();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void CheckAnswer()
    {
        
        //Debug.Log("Checking puzzle");

        S_ItemSlot[] slots =
            FindObjectsByType<S_ItemSlot>();

        System.Array.Sort(
            slots,
            (a, b) => a.slotIndex.CompareTo(b.slotIndex)
        );

        for (int i = 0; i < slots.Length; i++)
        {
            DragDrop[] words =
                slots[i].GetComponentsInChildren<DragDrop>();
            
            DragDrop word =
                slots[i].GetComponentInChildren<DragDrop>();

            if (word == null)
            {
                Debug.Log("Missing word!");
                return;
            }

            Debug.Log("Found: " + word.word);

            if (word.word != correctSentence[i])
            {
                Debug.Log("Wrong order!");
                return;
            }
        }

        Debug.Log("Puzzle Solved!");

        letterPuzzleUI.SetActive(false);
        completedLetterUI.SetActive(true);
    }
}
