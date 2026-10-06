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
        if (dialogueData == null || (S_PauseController.IsGamePaused && !isDialogueActive))
            return;

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

        dialogueUI.SetNPCInfo(
            dialogueData.npcName,
            dialogueData.npcPortrait
        );

        dialogueUI.ShowDialogueUI(true);

        S_PauseController.SetPauseState(true);

        StartCoroutine(TypeLine());
    }

    void NextLine()
    {
        // If the current line is still typing,
        // instantly finish the line.
        if (isTyping)
        {
            StopAllCoroutines();

            DialogueLine currentLine =
                dialogueData.dialogueLines[dialogueIndex];

            dialogueUI.SetDialogueText(currentLine.text);

            isTyping = false;
            return;
        }

        // Check if the current line should end the dialogue
        DialogueLine finishedLine =
            dialogueData.dialogueLines[dialogueIndex];

        if (finishedLine.endDialogue)
        {
            EndDialogue();
            return;
        }

        // Move to the next line
        dialogueIndex++;

        if (dialogueIndex < dialogueData.dialogueLines.Length)
        {
            StartCoroutine(TypeLine());
        }
        else
        {
            EndDialogue();
        }
    }

    IEnumerator TypeLine()
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

            // Uncomment when you want voice sounds:
            // SoundEffectManager.PlayVoice(
            //     dialogueData.voiceSound,
            //     dialogueData.voicePitch
            // );

            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }

        isTyping = false;

        // Automatically progress if this line has Auto Progress checked
        if (currentLine.autoProgress)
        {
            yield return new WaitForSeconds(
                dialogueData.autoProgressDelay
            );

            NextLine();
        }
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
