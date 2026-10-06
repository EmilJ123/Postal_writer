using System.Collections;
using UnityEngine;

public class S_NPC : MonoBehaviour, S_IInteractable 
{
    public S_NPCDialogue dialogueData;
    private S_DialogueController dialogueUI;
    private int dialogueIndex;
    private bool isTyping;
    private bool isDialogueActive;

    private void Start() 
    {
        dialogueUI = S_DialogueController.instance;
    }

    public bool CanInteract() 
    {
        return !isDialogueActive;
    }

    public void Interact() 
    {
        if (dialogueData == null || (S_PauseController.IsGamePaused && !isDialogueActive)) return;

        if (!isDialogueActive) 
        {
            StartDialogue();
        } 
        else 
        {
            NextLine();
        }
    }

    void StartDialogue() 
    {
        isDialogueActive = true;
        dialogueIndex = 0;
        dialogueUI.SetNPCInfo(dialogueData.npcName, dialogueData.npcPortrait);
        dialogueUI.ShowDialogueUI(true);
        S_PauseController.SetPauseState(true);
        DisplayCurrentLine();
    }

    void NextLine() 
    {

        if (isTyping) 
        {
            StopAllCoroutines();
            DialogueLine currentLine = dialogueData.dialogueLines[dialogueIndex];
            dialogueUI.SetDialogueText(currentLine.text);
            isTyping = false;
            return;
        }

        dialogueUI.ClearChoices();

        // Sjekk endDialogueLines
        if (dialogueData.endDialogueLines.Length > dialogueIndex && dialogueData.endDialogueLines[dialogueIndex]) 
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

   
        DialogueLine finishedLine = dialogueData.dialogueLines[dialogueIndex];
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

    IEnumerator TypeLine() 
    {
        isTyping = true;
        DialogueLine currentLine = dialogueData.dialogueLines[dialogueIndex];
        dialogueUI.SetDialogueText("");

        foreach (char letter in currentLine.text) 
        {
            dialogueUI.SetDialogueText(dialogueUI.dialogueText.text + letter);

            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }
        isTyping = false;


        if (currentLine.autoProgress) 
        {
            yield return new WaitForSeconds(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    void DisplayChoices(DialogueChoice choice) 
    {
        for (int i = 0; i < choice.choices.Length; i++) 
        {
            int nextIndex = choice.nextDialogueIndices[i];
            dialogueUI.CreateChoiceButton(choice.choices[i], () => ChooseOption(nextIndex));
        }
    }

    void ChooseOption(int nextIndex) 
    {
        dialogueIndex = nextIndex;
        dialogueUI.ClearChoices();
        StartCoroutine(TypeLine());
    }

    void DisplayCurrentLine() 
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
}
