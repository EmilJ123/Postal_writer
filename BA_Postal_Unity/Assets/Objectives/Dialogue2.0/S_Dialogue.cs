using UnityEngine;
using TMPro;
using System.Collections;

public class S_Dialogue : MonoBehaviour
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

    private int lineIndex = 0;

    private bool isInteracting = false;
    private bool canInteract = false;
    private bool isTyping = false;
    private bool choosing = false;

    private Coroutine typingCoroutine;


    private void Update()
    {
        // Show interact prompt
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(
                canInteract && !isInteracting
            );
        }


        // Start dialogue
        if (canInteract &&
            !isInteracting &&
            Input.GetKeyDown(KeyCode.F))
        {
            StartDialogue();
        }


        // If player is choosing an option
        if (choosing)
        {
            HandleChoiceInput();
            return;
        }


        // Continue dialogue
        if (isInteracting &&
            !isTyping &&
            nextLinePrompt != null &&
            nextLinePrompt.activeInHierarchy &&
            Input.GetKeyDown(KeyCode.F))
        {
            NextLine();
        }
    }


    private void StartDialogue()
    {
        if (dialogueData == null)
        {
            Debug.LogWarning(
                "No S_NPCDialogue assigned to " +
                gameObject.name
            );

            return;
        }

        if (dialogueData.dialogueLines == null ||
            dialogueData.dialogueLines.Length == 0)
        {
            Debug.LogWarning(
                "No dialogue lines found in " +
                dialogueData.name
            );

            return;
        }


        canInteract = false;
        isInteracting = true;

        lineIndex = 0;

        if (dialogueBox != null)
            dialogueBox.SetActive(true);

        if (nextLinePrompt != null)
            nextLinePrompt.SetActive(false);

        HideChoices();

        StartTypingLine();
    }


    private void StartTypingLine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine =
            StartCoroutine(WriteLine());
    }


    private IEnumerator WriteLine()
    {
        isTyping = true;

        if (nextLinePrompt != null)
            nextLinePrompt.SetActive(false);

        HideChoices();

        dialogueText.text = "";

        DialogueLine currentLine =
            dialogueData.dialogueLines[lineIndex];


        // Type the dialogue
        foreach (char character in currentLine.text)
        {
            dialogueText.text += character;

            if (dialogueData.typingSpeed > 0)
            {
                yield return new WaitForSeconds(
                    dialogueData.typingSpeed
                );
            }
        }


        isTyping = false;


        // Check if this line has choices
        DialogueChoice dialogueChoice =
            GetChoicesForLine(lineIndex);

        if (dialogueChoice != null &&
            dialogueChoice.choices != null &&
            dialogueChoice.choices.Length > 0)
        {
            ShowChoices(dialogueChoice);
            yield break;
        }


        // --------------------------------------------------
        // END DIALOGUE
        // --------------------------------------------------

        // If this specific line is marked "End Dialogue",
        // close the dialogue immediately after typing.
        if (currentLine.endDialogue)
        {
            EndDialogue();
            yield break;
        }


        // If this is the final line in the dialogue array,
        // also end the dialogue.
        if (lineIndex >= dialogueData.dialogueLines.Length - 1)
        {
            if (nextLinePrompt != null)
                nextLinePrompt.SetActive(true);

            yield break;
        }


        // Normal dialogue
        if (nextLinePrompt != null)
            nextLinePrompt.SetActive(true);


        // Auto progress
        if (currentLine.autoProgress)
        {
            yield return new WaitForSeconds(
                dialogueData.autoProgressDelay
            );

            if (isInteracting &&
                !choosing &&
                !isTyping)
            {
                NextLine();
            }
        }
    }


    private void NextLine()
    {
        if (isTyping || choosing)
            return;


        // Safety check
        if (lineIndex < 0 ||
            lineIndex >= dialogueData.dialogueLines.Length)
        {
            EndDialogue();
            return;
        }


        DialogueLine currentLine =
            dialogueData.dialogueLines[lineIndex];


        // IMPORTANT:
        // If this line is marked "End Dialogue",
        // do NOT advance to the next line.
        if (currentLine.endDialogue)
        {
            EndDialogue();
            return;
        }


        // If this is the final line, end dialogue.
        if (lineIndex >=
            dialogueData.dialogueLines.Length - 1)
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
        if (dialogueData.choices == null)
            return null;


        foreach (DialogueChoice choice
                 in dialogueData.choices)
        {
            if (choice.dialogueIndex == index)
            {
                return choice;
            }
        }


        return null;
    }


    private void ShowChoices(
        DialogueChoice dialogueChoice
    )
    {
        if (choicePanel == null)
        {
            Debug.LogWarning(
                "Choice Panel is not assigned."
            );

            return;
        }


        choosing = true;

        if (nextLinePrompt != null)
            nextLinePrompt.SetActive(false);


        choicePanel.SetActive(true);


        // Hide all choice text objects
        for (int i = 0; i < choiceTexts.Length; i++)
        {
            if (choiceTexts[i] != null)
            {
                choiceTexts[i].gameObject.SetActive(false);
            }
        }


        // Display choices
        for (int i = 0;
             i < dialogueChoice.choices.Length &&
             i < choiceTexts.Length;
             i++)
        {
            if (choiceTexts[i] == null)
                continue;


            Choice choice =
                dialogueChoice.choices[i];


            // Add number before choice
            choiceTexts[i].text =
                "[" + (i + 1) + "] " +
                choice.text;


            choiceTexts[i].gameObject.SetActive(true);
        }
    }


    private void HandleChoiceInput()
    {
        DialogueChoice dialogueChoice =
            GetChoicesForLine(lineIndex);


        if (dialogueChoice == null)
            return;


        // Choice 1
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SelectChoice(0);
        }


        // Choice 2
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SelectChoice(1);
        }


        // Choice 3
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SelectChoice(2);
        }


        // Choice 4
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SelectChoice(3);
        }


        // Choice 5
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            SelectChoice(4);
        }
    }


    private void SelectChoice(int choiceIndex)
    {
        DialogueChoice dialogueChoice =
            GetChoicesForLine(lineIndex);


        if (dialogueChoice == null)
            return;


        if (choiceIndex < 0 ||
            choiceIndex >= dialogueChoice.choices.Length)
        {
            return;
        }


        Choice selectedChoice =
            dialogueChoice.choices[choiceIndex];


        Debug.Log(
            "Selected choice: " +
            selectedChoice.text
        );


        // Give quest
        if (selectedChoice.givesQuest &&
            selectedChoice.quest != null)
        {
            GiveQuest(
                selectedChoice.quest
            );
        }


        // Stop choosing
        choosing = false;


        // Hide choices
        HideChoices();


        // Move to selected dialogue
        lineIndex =
            selectedChoice.nextDialogueIndex;


        // Safety check for invalid dialogue index
        if (lineIndex < 0 ||
            lineIndex >= dialogueData.dialogueLines.Length)
        {
            Debug.LogWarning(
                "Choice '" +
                selectedChoice.text +
                "' points to an invalid dialogue index: " +
                lineIndex
            );

            EndDialogue();
            return;
        }


        StartTypingLine();
    }


    private void HideChoices()
    {
        choosing = false;


        if (choicePanel != null)
            choicePanel.SetActive(false);


        if (choiceTexts == null)
            return;


        foreach (TMP_Text text in choiceTexts)
        {
            if (text != null)
            {
                text.gameObject.SetActive(false);
            }
        }
    }


    // --------------------------------------------------
    // QUEST
    // --------------------------------------------------

    private void GiveQuest(
        S_QuestManager quest
    )
    {
        Debug.Log(
            "Quest selected: " +
            quest.name
        );


        // Connect your quest system here.
        //
        // Example:
        // quest.StartQuest();
    }


    // --------------------------------------------------
    // END DIALOGUE
    // --------------------------------------------------

    private void EndDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }


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


    // --------------------------------------------------
    // PLAYER TRIGGER
    // --------------------------------------------------

    private void OnTriggerEnter(
        Collider collision
    )
    {
        if (collision.CompareTag("Player") &&
            !isInteracting)
        {
            canInteract = true;
        }
    }


    private void OnTriggerExit(
        Collider collision
    )
    {
        if (collision.CompareTag("Player"))
        {
            canInteract = false;
        }
    }
}