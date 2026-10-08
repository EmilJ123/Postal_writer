using System.Collections;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class S_NPC : MonoBehaviour, S_IInteractable
{
    public S_NPCDialogue dialogueData;

    private S_DialogueController dialogueUI;
    private int dialogueIndex;
    private bool isTyping;
    private bool isDialogueActive;

    private bool playerInRange;

    private void Start()
    {
        dialogueUI = S_DialogueController.instance;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (isDialogueActive)
            {
                NextLine();
                return;
            }

            if (playerInRange && CanInteract())
            {
                Interact();
            }
        }
    }

    public bool CanInteract()
    {
        return true;
    }

    public void Interact()
    {
        if (dialogueData == null)
            return;

        if (!isDialogueActive)
        {
            if (S_PauseController.IsGamePaused)
                return;

            StartDialogue();
        }
        else
        {
            NextLine();
        }
    }

    private void StartDialogue()
    {
        // Reset dialogue so it starts from the beginning every time
        dialogueIndex = 0;
        isDialogueActive = true;
        isTyping = false;

        dialogueUI.SetNPCInfo(
            dialogueData.npcName
        );

        dialogueUI.ClearChoices();
        dialogueUI.ShowDialogueUI(true);

        S_PauseController.SetPauseState(true);

        // Unlock mouse for dialogue choices
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        DisplayCurrentLine();
    }

    private void NextLine()
    {
        if (isTyping)
        {
            StopAllCoroutines();

            DialogueLine currentLine =
                dialogueData.dialogueLines[dialogueIndex];

            dialogueUI.SetDialogueText(currentLine.text);

            isTyping = false;
            return;
        }

        dialogueUI.ClearChoices();

        if (dialogueData.endDialogueLines.Length > dialogueIndex &&
            dialogueData.endDialogueLines[dialogueIndex])
        {
            EndDialogue();
            return;
        }

        foreach (DialogueChoice choice in dialogueData.choices)
        {
            if (choice.dialogueIndex == dialogueIndex)
            {
                DisplayChoices(choice);
                return;
            }
        }

        DialogueLine finishedLine =
            dialogueData.dialogueLines[dialogueIndex];

        if (finishedLine.endDialogue)
        {
            EndDialogue();
            return;
        }

        dialogueIndex++;

        if (dialogueIndex < dialogueData.dialogueLines.Length)
        {
            DisplayCurrentLine();
        }
        else
        {
            EndDialogue();
        }
    }

    private IEnumerator TypeLine()
    {
        isTyping = true;

        DialogueLine currentLine =
            dialogueData.dialogueLines[dialogueIndex];

        dialogueUI.SetDialogueText("");

        foreach (char letter in currentLine.text)
        {
            dialogueUI.SetDialogueText(
                dialogueUI.dialogueText.text + letter
            );

            yield return new WaitForSecondsRealtime(
                dialogueData.typingSpeed
            );
        }

        isTyping = false;

        if (currentLine.autoProgress)
        {
            yield return new WaitForSecondsRealtime(
                dialogueData.autoProgressDelay
            );

            NextLine();
        }
    }

    private void DisplayChoices(DialogueChoice choice)
    {
        for (int i = 0; i < choice.choices.Length; i++)
        {
            int nextIndex = choice.nextDialogueIndices[i];

            dialogueUI.CreateChoiceButton(
                choice.choices[i],
                () => ChooseOption(nextIndex)
            );
        }
    }

    private void ChooseOption(int nextIndex)
    {
        dialogueIndex = nextIndex;

        dialogueUI.ClearChoices();

        StartCoroutine(TypeLine());
    }

    private void DisplayCurrentLine()
    {
        StopAllCoroutines();
        StartCoroutine(TypeLine());
    }

    public void EndDialogue()
    {
        StopAllCoroutines();

        isTyping = false;
        isDialogueActive = false;

        dialogueIndex = 0;

        dialogueUI.SetDialogueText("");
        dialogueUI.ClearChoices();
        dialogueUI.ShowDialogueUI(false);

        S_PauseController.SetPauseState(false);

        // Lock mouse again for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            Debug.Log("Player is in range of NPC!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
