
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
    
    private int lineIndex;

    private bool isInteracting;
    private bool canInteract;
    private bool isTyping;
    private bool choosing;

    private Coroutine typingCoroutine;
    private Coroutine autoProgressCoroutine;

    
    private void Update()
    {
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(
                canInteract && !isInteracting
            );
        }

        if (!isInteracting)
        {
            if (canInteract && Input.GetKeyDown(KeyCode.F))
            {
                StartDialogue();
            }

            return;
        }

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

        if (dialogueData.dialogueLines == null ||
            dialogueData.dialogueLines.Length == 0)
        {
            Debug.LogWarning(
                "No dialogue lines found on " + dialogueData.name
            );
            return;
        }

        canInteract = true;
        isInteracting = true;
        lineIndex = 0;

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
        Debug.Log(
            $"Choice {choiceIndex} selected. Current line={lineIndex}"
        );

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

        Debug.Log("Selected choice: " + selectedChoice);

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

        Debug.Log(
            $"Moving from line {lineIndex} to line {nextDialogueIndex}"
        );

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

        HideChoices();

        if (interactPrompt != null)
            interactPrompt.SetActive(canInteract);
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
}
