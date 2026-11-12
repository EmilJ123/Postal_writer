
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

    private enum QuestState{ NotStarted, InProgress, Completed}
    private QuestState questState = QuestState.NotStarted;
    private Coroutine typingCoroutine;

    private void Start()
    {
        dialogueUI = S_DialogueController.instance;

        if (dialogueUI == null)
        {
            Debug.LogError(
                "S_DialogueController instance was not found!",
                this
            );
        }
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.F))
            return;

        // Continue or skip text if dialogue is active.
        if (isDialogueActive)
        {
            NextLine();
            return;
        }

        // Start dialogue only when the player is nearby.
        if (playerInRange && CanInteract())
        {
            Interact();
        }
    }

    public bool CanInteract()
    {
        return dialogueData != null
            && dialogueUI != null
            && !S_PauseController.IsGamePaused;
    }

    public void Interact()
    {
        if (dialogueData == null || dialogueUI == null)
            return;

        if (isDialogueActive)
        {
            NextLine();
            return;
        }

        if (!playerInRange || S_PauseController.IsGamePaused)
            return;

        StartDialogue();
    }

    private void StartDialogue()
    {
        //Sync with quest Data
        SyncQuestState();
        //Set dialogue line based on questState
        if (questState == QuestState.NotStarted)
        {
            dialogueIndex = 0;
        }
        else if (questState == QuestState.InProgress)
        {
            dialogueIndex = 1;
        }
        else if (questState == QuestState.Completed)
        {
            dialogueIndex = dialogueData.questCompletedIndex; //continue this part on a later part
        }
        
        if (dialogueData.dialogueLines == null ||
            dialogueData.dialogueLines.Length == 0)

        //dialogueIndex = 0;
        isDialogueActive = true;
        isTyping = false;

        dialogueUI.SetNPCInfo(dialogueData.npcName);
        dialogueUI.SetDialogueText("");
        dialogueUI.ClearChoices();
        dialogueUI.ShowDialogueUI(true);

        S_PauseController.SetPauseState(true);

        // Unlock the cursor for dialogue interaction.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        DisplayCurrentLine();
    }

    private void SyncQuestState()
    {
        if (dialogueData.quest == null) return;

        string questID = dialogueData.quest.questID;
        if (S_QuestController.Instance.IsQuestCompleted(questID) || S_QuestController.Instance.IsQuestHandedIn(questID)) //no idea if this part works 
        {
            questState = QuestState.Completed;
        }
        else if (S_QuestController.Instance.IsQuestActive(questID))
        {
            questState = QuestState.InProgress;
        }
        else
        {
            questState = QuestState.NotStarted;
        }
    }
    private void NextLine()
    {
        if (!isDialogueActive)
            return;

        // First press: finish displaying the current line.
        if (isTyping)
        {
            FinishTyping();
            return;
        }

        // Guard against invalid dialogue indices.
        if (!IsValidDialogueIndex(dialogueIndex))
        {
            EndDialogue();
            return;
        }

        dialogueUI.ClearChoices();

        DialogueLine currentLine =
            dialogueData.dialogueLines[dialogueIndex];

        // Check the optional end-dialogue array safely.
        if (dialogueData.endDialogueLines != null &&
            dialogueIndex < dialogueData.endDialogueLines.Length &&
            dialogueData.endDialogueLines[dialogueIndex])
        {
            EndDialogue();
            return;
        }

        // Show choices attached to this dialogue line.
        if (dialogueData.choices != null)
        {
            foreach (DialogueChoice choice in dialogueData.choices)
            {
                if (choice == null)
                    continue;

                if (choice.dialogueIndex == dialogueIndex)
                {
                    DisplayChoices(choice);
                    return;
                }
            }
        }

        // End if this line is marked as the final line.
        if (currentLine.endDialogue)
        {
            EndDialogue();
            return;
        }

        dialogueIndex++;

        if (IsValidDialogueIndex(dialogueIndex))
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
        if (!IsValidDialogueIndex(dialogueIndex))
        {
            EndDialogue();
            yield break;
        }

        isTyping = true;

        DialogueLine currentLine =
            dialogueData.dialogueLines[dialogueIndex];

        string fullText = currentLine.text ?? "";

        dialogueUI.SetDialogueText("");

        foreach (char letter in fullText)
        {
            if (!isDialogueActive)
                yield break;

            dialogueUI.SetDialogueText(
                dialogueUI.dialogueText.text + letter
            );

            yield return new WaitForSecondsRealtime(
                Mathf.Max(0f, dialogueData.typingSpeed)
            );
        }

        isTyping = false;
        typingCoroutine = null;

        if (currentLine.autoProgress && isDialogueActive)
        {
            yield return new WaitForSecondsRealtime(
                Mathf.Max(0f, dialogueData.autoProgressDelay)
            );

            if (isDialogueActive && !isTyping)
            {
                NextLine();
            }
        }
    }

    private void FinishTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (!IsValidDialogueIndex(dialogueIndex))
        {
            isTyping = false;
            EndDialogue();
            return;
        }

        DialogueLine currentLine =
            dialogueData.dialogueLines[dialogueIndex];

        dialogueUI.SetDialogueText(currentLine.text ?? "");

        isTyping = false;
    }

    private void DisplayChoices(DialogueChoice choice)
    {
        if (choice.choices == null ||
            choice.nextDialogueIndices == null ||
            choice.choices.Length != choice.nextDialogueIndices.Length)
        {
            Debug.LogWarning(
                "Dialogue choices and next dialogue indices must match.",
                this
            );
            EndDialogue();
            return;
        }

        if (choice.choices.Length == 0)
        {
            EndDialogue();
            return;
        }

        for (int i = 0; i < choice.choices.Length; i++)
        {
            int nextIndex = choice.nextDialogueIndices[i];
            bool givesQuest = choice.givesQuest[i];
            // Capture the current index for this button.
            string choiceText = choice.choices[i];
            dialogueUI.CreateChoiceButton(choiceText,() => ChooseOption(nextIndex, givesQuest));
        }
    }

    
    private void ChooseOption(int nextIndex, bool givesQuest)
    {
        if (givesQuest)
        {
            Debug.Log("Quest given? " + dialogueData.quest);
            S_QuestController.Instance.AcceptQuest(dialogueData.quest);
            questState = QuestState.InProgress;
        }
        if (!isDialogueActive)
            return;

        if (!IsValidDialogueIndex(nextIndex))
        {
            Debug.LogWarning(
                "A dialogue choice points to an invalid dialogue index: "
                + nextIndex,
                this
            );

            EndDialogue();
            return;
        }

        StopTyping();

        dialogueIndex = nextIndex;
        dialogueUI.ClearChoices();
        DisplayCurrentLine();
    }

    private void DisplayCurrentLine()
    {
        if (!isDialogueActive)
            return;

        if (!IsValidDialogueIndex(dialogueIndex))
        {
            EndDialogue();
            return;
        }

        StopTyping();

        typingCoroutine = StartCoroutine(TypeLine());
    }

    private bool IsValidDialogueIndex(int index)
    {
        return dialogueData != null
            && dialogueData.dialogueLines != null
            && index >= 0
            && index < dialogueData.dialogueLines.Length;
    }

    private void StopTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;
    }

    public void EndDialogue()
    {
        if (questState == QuestState.Completed &&
            !S_QuestController.Instance.IsQuestHandedIn(dialogueData.quest.questID))
        {
            //HandleQuestCompletion
            HandleQuestCompletion(dialogueData.quest);
        }
        
        if (!isDialogueActive)
            return;

        StopTyping();

        isDialogueActive = false;
        dialogueIndex = 0;

        dialogueUI.SetDialogueText("");
        dialogueUI.ClearChoices();
        dialogueUI.ShowDialogueUI(false);

        S_PauseController.SetPauseState(false);

        // Restore the gameplay cursor.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void HandleQuestCompletion(S_QuestManager quest)
    {
        S_QuestController.Instance.HandInQuest(quest.questID);
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

    private void OnDisable()
    {
        StopTyping();
    }
}