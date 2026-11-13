
using System.Collections;
using UnityEngine;
using TMPro;

public class S_Dialogue3 : MonoBehaviour
{
    [Header("Dialogue Data")]
    [SerializeField] private S_NPCDialogue dialogueData;

    [Header("UI")]
    [SerializeField] private GameObject interactPrompt;
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject nextLinePrompt;

    [Header("Choice UI")]
    [SerializeField] private GameObject choicePanel;
    [SerializeField] private TMP_Text[] choiceTexts;
    
    [Header("Quest Turn-In(optional)")]
    [SerializeField] private S_QuestManager turnInQuest;
    [SerializeField] private int turnInLineIndex;
    
    private int lineIndex;
    private static S_Dialogue3 activeNPC;

    private bool isInteracting;
    private bool canInteract;
    private bool isTyping;
    private bool choosing;
    
    
    private enum QuestState
    {
        NotStarted,
        InProgress,
        Completed
    }

    private QuestState questState = QuestState.NotStarted;
    
    private Coroutine typingCoroutine;
    private Coroutine autoProgressCoroutine;

    
    private void Update()
    {
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(canInteract && !isInteracting && activeNPC == null);
        }

        if (!isInteracting)
        {
            if (canInteract && Input.GetKeyDown(KeyCode.F))
            {
                StartDialogue();
            }

            return;
        }
        if (activeNPC != this)
            return;

        if (choosing)
        {
            HandleChoiceInput();
            return;
        }

        if (isTyping)
        {
            // Press F to instantly finish typing the current line.
            if (Input.GetKeyDown(KeyCode.F))
            {
                CompleteTyping();
            }

            return;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            NextLine();
        }
    }

    // --------------------------------------------------
    // START DIALOGUE
    // --------------------------------------------------

    // ReSharper disable Unity.PerformanceAnalysis
    private void StartDialogue()
    {
        
        if (dialogueData == null)
        {
            Debug.LogWarning(
                "No S_NPCDialogue assigned to " + gameObject.name
            );
            return;
        }
        SyncQuestState();

        if (dialogueData.dialogueLines == null ||
            dialogueData.dialogueLines.Length == 0)
        {
            Debug.LogWarning(
                "No dialogue lines found on " + dialogueData.name
            );
            return;
        }
        if (activeNPC != null && activeNPC != this)
            return;
        
        activeNPC = this;
        //canInteract = true;
        isInteracting = true;

        switch (questState)
        {
            case QuestState.NotStarted:
            default:
                lineIndex = 0;
                break;

            case QuestState.InProgress:
                lineIndex = Mathf.Clamp(
                    dialogueData.questInProgressIndex,
                    0,
                    dialogueData.dialogueLines.Length - 1
                );
                break;

            case QuestState.Completed:
                bool isHandingInNow = turnInQuest != null &&
                                      S_QuestController.Instance != null &&
                                      S_QuestController.Instance.IsQuestCompleted(turnInQuest.questID);

                int targetIndex = isHandingInNow
                    ? turnInLineIndex
                    : dialogueData.questCompletedIndex;

                lineIndex = Mathf.Clamp(
                    targetIndex,
                    0,
                    dialogueData.dialogueLines.Length - 1
                );
                break;
        }

        if (dialogueBox != null)
            dialogueBox.SetActive(true);

        if (nextLinePrompt != null)
            nextLinePrompt.SetActive(false);

        HideChoices();
        StartTypingLine();
    }

    // --------------------------------------------------
    // TYPEWRITER EFFECT
    // --------------------------------------------------

    private void StartTypingLine()
    {
        StopTypingCoroutine();
        StopAutoProgress();

        if (!IsValidLineIndex(lineIndex))
        {
            EndDialogue();
            return;
        }

        typingCoroutine = StartCoroutine(WriteLine());
    }

    private IEnumerator WriteLine()
    {
        isTyping = true;

        if (nextLinePrompt != null)
            nextLinePrompt.SetActive(false);

        HideChoices();

        if (dialogueText == null)
        {
            Debug.LogWarning("Dialogue Text is not assigned.");
            EndDialogue();
            yield break;
        }

        DialogueLine currentLine =
            dialogueData.dialogueLines[lineIndex];
        Debug.Log($"[{gameObject.name}] Typing lineIndex: {lineIndex} | Text: '{currentLine.text}' | autoProgress: {currentLine.autoProgress} | endDialogue: {currentLine.endDialogue}");
        dialogueText.text = "";

        string fullText = currentLine.text ?? "";

        foreach (char character in fullText)
        {
            if (!isInteracting)
                yield break;

            dialogueText.text += character;

            if (dialogueData.typingSpeed > 0f)
            {
                yield return new WaitForSeconds(
                    dialogueData.typingSpeed
                );
            }
        }

        isTyping = false;
        typingCoroutine = null;

        // Check for choices on this line.
        DialogueChoice dialogueChoice =
            GetChoicesForLine(lineIndex);

        if (dialogueChoice != null &&
            dialogueChoice.choices != null &&
            dialogueChoice.choices.Length > 0)
        {
            ShowChoices(dialogueChoice);
            yield break;
        }

        // End immediately if this line is marked as the ending.
        if (currentLine.endDialogue)
        {
            if (nextLinePrompt != null)
                nextLinePrompt.SetActive(true);

            yield break;
        }

        // Keep F available on the final line so the player
        // can close the conversation.
        if (nextLinePrompt != null)
            nextLinePrompt.SetActive(true);

        // Automatically advance only when another line exists.
        if (currentLine.autoProgress &&
            lineIndex < dialogueData.dialogueLines.Length - 1)
        {
            autoProgressCoroutine = StartCoroutine(
                AutoProgressAfterDelay()
            );
        }
    }

    private void CompleteTyping()
    {
        if (!isTyping || !IsValidLineIndex(lineIndex))
            return;

        StopTypingCoroutine();

        DialogueLine currentLine =
            dialogueData.dialogueLines[lineIndex];

        if (dialogueText != null)
            dialogueText.text = currentLine.text ?? "";

        isTyping = false;

        // Run the post-typing logic through a new coroutine.
        typingCoroutine = StartCoroutine(FinishLine());
    }

    private IEnumerator FinishLine()
    {
        if (!isInteracting || !IsValidLineIndex(lineIndex))
            yield break;

        DialogueLine currentLine =
            dialogueData.dialogueLines[lineIndex];

        DialogueChoice dialogueChoice =
            GetChoicesForLine(lineIndex);

        if (dialogueChoice != null &&
            dialogueChoice.choices != null &&
            dialogueChoice.choices.Length > 0)
        {
            typingCoroutine = null;
            ShowChoices(dialogueChoice);
            yield break;
        }

        if (currentLine.endDialogue)
        {
            if (nextLinePrompt != null)
                nextLinePrompt.SetActive(true);

            typingCoroutine = null;
            yield break;
        }

        if (nextLinePrompt != null)
            nextLinePrompt.SetActive(true);

        typingCoroutine = null;

        if (currentLine.autoProgress &&
            lineIndex < dialogueData.dialogueLines.Length - 1)
        {
            autoProgressCoroutine = StartCoroutine(
                AutoProgressAfterDelay()
            );
        }
    }

    private IEnumerator AutoProgressAfterDelay()
    {
        yield return new WaitForSeconds(
            Mathf.Max(0f, dialogueData.autoProgressDelay)
        );

        autoProgressCoroutine = null;

        if (isInteracting && !isTyping && !choosing)
        {
            NextLine();
        }
    }

    // --------------------------------------------------
    // DIALOGUE PROGRESSION
    // --------------------------------------------------

    private void NextLine()
    {
        if (!isInteracting || isTyping || choosing)
            return;

        if (!IsValidLineIndex(lineIndex))
        {
            EndDialogue();
            return;
        }

        DialogueLine currentLine =
            dialogueData.dialogueLines[lineIndex];

        if (currentLine.endDialogue ||
            lineIndex >= dialogueData.dialogueLines.Length - 1)
        {
            EndDialogue();
            return;
        }

        lineIndex++;
        StartTypingLine();
    }

    // --------------------------------------------------
    // CHOICES
    // --------------------------------------------------

    private DialogueChoice GetChoicesForLine(int index)
    {
        if (dialogueData == null || dialogueData.choices == null)
            return null;

        foreach (DialogueChoice dialogueChoice in dialogueData.choices)
        {
            if (dialogueChoice != null &&
                dialogueChoice.dialogueIndex == index)
            {
                return dialogueChoice;
            }
        }

        return null;
    }

    private void ShowChoices(DialogueChoice dialogueChoice)
    {
        if (choicePanel == null)
        {
            Debug.LogWarning("Choice Panel is not assigned.");
            return;
        }

        if (choiceTexts == null || choiceTexts.Length == 0)
        {
            Debug.LogWarning("No choice text slots are assigned.");
            return;
        }

        choosing = true;

        if (nextLinePrompt != null)
            nextLinePrompt.SetActive(false);

        choicePanel.SetActive(true);

        for (int i = 0; i < choiceTexts.Length; i++)
        {
            if (choiceTexts[i] != null)
                choiceTexts[i].gameObject.SetActive(false);
        }

        int displayedCount = Mathf.Min(
            dialogueChoice.choices.Length,
            choiceTexts.Length,
            5
        );

        for (int i = 0; i < displayedCount; i++)
        {
            if (choiceTexts[i] == null)
                continue;

            choiceTexts[i].text =
                "[" + (i + 1) + "] " + dialogueChoice.choices[i];

            choiceTexts[i].gameObject.SetActive(true);
        }
    }

    private void HandleChoiceInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) ||
            Input.GetKeyDown(KeyCode.Keypad1))
            SelectChoice(0);

        else if (Input.GetKeyDown(KeyCode.Alpha2) ||
                 Input.GetKeyDown(KeyCode.Keypad2))
            SelectChoice(1);

        else if (Input.GetKeyDown(KeyCode.Alpha3) ||
                 Input.GetKeyDown(KeyCode.Keypad3))
            SelectChoice(2);

        else if (Input.GetKeyDown(KeyCode.Alpha4) ||
                 Input.GetKeyDown(KeyCode.Keypad4))
            SelectChoice(3);

        else if (Input.GetKeyDown(KeyCode.Alpha5) ||
                 Input.GetKeyDown(KeyCode.Keypad5))
            SelectChoice(4);
    }

    private void SelectChoice(int choiceIndex)
    {
        //Debug.Log(
            //$"Choice {choiceIndex} selected. Current line={lineIndex}"
        //);

        if (!choosing)
            return;

        DialogueChoice dialogueChoice =
            GetChoicesForLine(lineIndex);

        if (dialogueChoice == null ||
            dialogueChoice.choices == null ||
            choiceIndex < 0 ||
            choiceIndex >= dialogueChoice.choices.Length)
        {
            return;
        }

        string selectedChoice =
            dialogueChoice.choices[choiceIndex];

        //Debug.Log("Selected choice: " + selectedChoice);

        if (dialogueChoice.nextDialogueIndices == null ||
            choiceIndex >= dialogueChoice.nextDialogueIndices.Length)
        {
            Debug.LogWarning(
                "No next dialogue index configured for choice " + choiceIndex
            );

            EndDialogue();
            return;
        }

       
        int nextDialogueIndex =
            dialogueChoice.nextDialogueIndices[choiceIndex];
        
        if (dialogueChoice.givesQuest != null &&
            choiceIndex < dialogueChoice.givesQuest.Length &&
            dialogueChoice.givesQuest[choiceIndex])
        {
            Debug.Log("Giving quest!");

            if (dialogueData.quest != null)
            {
                S_QuestController.Instance.AcceptQuest(dialogueData.quest);
            }
            else
            {
                Debug.LogWarning("No quest assigned in NPC Dialogue!");
            }
        }

        choosing = false;
        HideChoices();

        lineIndex = nextDialogueIndex;
        StartTypingLine();
    }

    private void HideChoices()
    {
        choosing = false;

        if (choicePanel != null)
            choicePanel.SetActive(false);

        if (choiceTexts == null)
            return;

        foreach (TMP_Text choiceText in choiceTexts)
        {
            if (choiceText != null)
                choiceText.gameObject.SetActive(false);
        }
    }

    // --------------------------------------------------
    // QUEST
    // --------------------------------------------------

    private void GiveQuest(S_QuestManager quest)
    {
        if (quest == null)
            return;

        Debug.Log("Quest selected: " + quest.name);

        // Connect your quest manager here.
        // Example: quest.StartQuest();
    }

    // --------------------------------------------------
    // END DIALOGUE
    // --------------------------------------------------

    private void EndDialogue()
    {
        StopTypingCoroutine();
        StopAutoProgress();

        isTyping = false;
        choosing = false;
        isInteracting = false;

        if (dialogueBox != null)
            dialogueBox.SetActive(false);

        if (nextLinePrompt != null)
            nextLinePrompt.SetActive(false);
        
        if (activeNPC == this)
        {
            activeNPC = null;
        }
        HideChoices();

        if (interactPrompt != null)
            interactPrompt.SetActive(canInteract);
        
        if (questState == QuestState.Completed &&
            dialogueData.quest != null &&
            !S_QuestController.Instance.IsQuestHandedIn(dialogueData.quest.questID))
        {
            S_QuestController.Instance.HandInQuest(dialogueData.quest.questID);
        }
        
        S_QuestManager questToHandIn = (turnInQuest != null)
            ? turnInQuest
            : (dialogueData != null ? dialogueData.quest : null);

        if (questState == QuestState.Completed &&
            questToHandIn != null &&
            !S_QuestController.Instance.IsQuestHandedIn(questToHandIn.questID))
        {
            S_QuestController.Instance.HandInQuest(questToHandIn.questID);
        }
    }

    private void StopTypingCoroutine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
    }

    private void StopAutoProgress()
    {
        if (autoProgressCoroutine != null)
        {
            StopCoroutine(autoProgressCoroutine);
            autoProgressCoroutine = null;
        }
    }

    private bool IsValidLineIndex(int index)
    {
        return dialogueData != null &&
               dialogueData.dialogueLines != null &&
               index >= 0 &&
               index < dialogueData.dialogueLines.Length;
    }

    // --------------------------------------------------
    // PLAYER TRIGGER
    // --------------------------------------------------

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            canInteract = true;
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            canInteract = false;

            // Close the dialogue if the player walks away.
            if (isInteracting)
                EndDialogue();
        }
    }
    private void SyncQuestState()
    {
        questState = QuestState.NotStarted;

        if (S_QuestController.Instance == null)
            return;

        
        if (turnInQuest != null)
        {
            string turnInID = turnInQuest.questID;

            if (S_QuestController.Instance.IsQuestCompleted(turnInID))
            {
                questState = QuestState.Completed;
                return;
            }
        }

        
        if (dialogueData != null && dialogueData.quest != null)
        {
            string questID = dialogueData.quest.questID;

            if (S_QuestController.Instance.IsQuestHandedIn(questID))
            {
                
                questState = QuestState.Completed;
            }
            else if (S_QuestController.Instance.IsQuestCompleted(questID))
            {
              
                questState = (turnInQuest == dialogueData.quest)
                    ? QuestState.Completed
                    : QuestState.InProgress;
            }
            else if (S_QuestController.Instance.IsQuestActive(questID))
            {
                questState = QuestState.InProgress;
            }
        }
    }

    private void OnDisable()
    {
        if (activeNPC == this)
        {
            activeNPC = null;
        }
    }
}
