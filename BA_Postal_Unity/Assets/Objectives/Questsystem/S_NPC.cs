using System.Collections;
using UnityEngine;

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
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            if (CanInteract())
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
        isDialogueActive = true;
        dialogueIndex = 0;

        dialogueUI.SetNPCInfo(
            dialogueData.npcName,
            dialogueData.npcPortrait
        );

        dialogueUI.ShowDialogueUI(true);

        S_PauseController.SetPauseState(true);

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

            yield return new WaitForSeconds(
                dialogueData.typingSpeed
            );
        }

        isTyping = false;

        if (currentLine.autoProgress)
        {
            yield return new WaitForSeconds(
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

        dialogueUI.SetDialogueText("");
        dialogueUI.ShowDialogueUI(false);

        S_PauseController.SetPauseState(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            Debug.Log("Player is in range of NPC!");
            playerInRange = true;
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
