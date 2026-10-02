using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class S_NPC : MonoBehaviour, S_IInteractable
{
    public S_NPCDialogue dialogueData;
    public GameObject dialoguePanel;
    public TMP_Text dialogueText, nameText;
    public Image portraitImage;
    
    private int dialogueIndex;
    private bool isTyping, isDialogueActive;

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
            //nextline
        }
        else
        {
            //startdialogue
        }

    }
        
        void StartDialogue()
    {
        isDialogueActive = true;
        dialogueIndex = 0;

        nameText.text = dialogueData.npcName;
        portraitImage.sprite = dialogueData.npcPortrait;

        dialoguePanel.SetActive(true);
        S_PauseController.SetPauseState(true);

    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.SetText("");

        foreach (char letter in dialogueData.dialogueLines[dialogueIndex])
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }

        isTyping = false;

        if(dialogueData.autoProgressLines.Length > dialogueIndex && dialogueData.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSeconds(dialogueData.autoProgressDelay);
        }
    }
}
