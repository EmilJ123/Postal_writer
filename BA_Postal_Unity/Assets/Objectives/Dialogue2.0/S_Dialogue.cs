using UnityEngine;
using TMPro;
using System.Collections;

public class S_Dialogue : MonoBehaviour
{
    [Header("Dialogue Data")]
    [SerializeField] private S_NPCDialogue dialogueData;

    [Header("UI")]
    [SerializeField] private GameObject interactPrompt;
    [SerializeField] private TMP_Text nameText;
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

        if (choosing)
        {
            HandleChoiceInput();
            return;
        }

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

        nameText.SetText(dialogueData.name);

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


        if (currentLine.endDialogue)
        {
            EndDialogue();
            yield break;
        }

        if (lineIndex >= dialogueData.dialogueLines.Length - 1)
        {
            if (nextLinePrompt != null)
                nextLinePrompt.SetActive(true);

            yield break;
        }

        if (nextLinePrompt != null)
            nextLinePrompt.SetActive(true);

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
        if (lineIndex < 0 ||
            lineIndex >= dialogueData.dialogueLines.Length)
        {
            EndDialogue();
            return;
        }


        DialogueLine currentLine =
            dialogueData.dialogueLines[lineIndex];


        // When market "end dialogue" the dialogue ends, making it the last line
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

        for (int i = 0; i < choiceTexts.Length; i++)
        {
            if (choiceTexts[i] != null)
            {
                choiceTexts[i].gameObject.SetActive(false);
            }
        }

        for (int i = 0;
             i < dialogueChoice.choices.Length &&
             i < choiceTexts.Length;
             i++)
        {
            if (choiceTexts[i] == null)
                continue;


            Choice choice =
                dialogueChoice.choices[i];

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

        choosing = false;

        HideChoices();



        lineIndex =
            selectedChoice.nextDialogueIndex;


   
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
    // ADD QUEST 
    // --------------------------------------------------

    private void GiveQuest(
        S_QuestManager quest
    )
    {
        Debug.Log(
            "Quest selected: " +
            quest.name
        );
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
    // INTERACTION NO TOUCHY TOUCHY
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