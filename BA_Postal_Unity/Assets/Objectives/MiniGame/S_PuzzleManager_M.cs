using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
public class S_PuzzleManager_M : MonoBehaviour
{
    [SerializeField] private GameObject letterPuzzleUI;
    [SerializeField] private GameObject completedLetterUI;
    [SerializeField] private GameObject[] objectsToEnableOnComplete;
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
    public void OpenPuzzle()
    {
        letterPuzzleUI.SetActive(true);

        EnterPuzzleMode();
    }
    
    public void ClosePuzzle()
    {
        letterPuzzleUI.SetActive(false);
        ExitPuzzleMode();
    }
    public void ContinueConversation()
    {
        completedLetterUI.SetActive(false);
        
        //FindObjectsByType<S_Dialogue3>().ContinueDialogue();
    }
    
    private void EnterPuzzleMode()
    {
        Debug.Log("Entering Puzzle Mode");

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        cameraInput.enabled = false;

        playerInput.actions.FindActionMap("Gameplay").Disable();

        Debug.Log(
            "Gameplay Enabled: " +
            playerInput.actions.FindActionMap("Gameplay").enabled
        );
    }

    private void ExitPuzzleMode()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        cameraInput.enabled = true;
                
        playerInput.actions.FindActionMap("Gameplay").Enable();
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
               // Debug.Log("Missing word!");
                return;
            }

            //  Debug.Log("Found: " + word.word);

            if (word.word != correctSentence[i])
            {
             //   Debug.Log("Wrong order!");
                return;
            }
        }

        Debug.Log("Puzzle Solved!");

        ClosePuzzle();
        completedLetterUI.SetActive(true);
        { 
            foreach (GameObject obj in objectsToEnableOnComplete)
            {
                if (obj != null)
                    obj.SetActive(true);
            }
        }
    }
}
