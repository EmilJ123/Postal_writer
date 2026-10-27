
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class S_DialogueController : MonoBehaviour
{
    [Header("Dialogue Data")]
    [SerializeField] private S_NPCDialogue dialogueData;

    [Header("UI")]
    [SerializeField] private GameObject InteractPromt;
    [SerializeField] private GameObject DialogueBox;
    [SerializeField] private TMP_Text DialogueText;
    [SerializeField] private GameObject NextLinePrompt;

    [Header("Choices")]
    public Transform choicesContainer;
    public GameObject choiceButtonPrefab;

    private int lineIndex = 0;

    private bool isInteracting = false;
    private bool canInteract = false;
    private bool isTyping = false;

    private Coroutine typingCoroutine;
    private Coroutine autoProgressCoroutine;

    private enum QuestState
    {
        NotStarted, InProgress, Completed
    }

    private QuestState questState = QuestState.NotStarted;

    private void Start()
    {
        if (InteractPromt != null)
            InteractPromt.SetActive(false);

        if (DialogueBox != null)
            DialogueBox.SetActive(false);

        if (NextLinePrompt != null)
            NextLinePrompt.SetActive(false);
    }

    private void Update()
    {
        if (InteractPromt != null)
            InteractPromt.SetActive(canInteract && !isInteracting);

        if (canInteract && !isInteracting &&
            Input.GetKeyDown(KeyCode.F))
        {
            StartDialogue();
        }
        else if (isInteracting &&
                 !isTyping &&
                 NextLinePrompt != null &&
                 NextLinePrompt.activeInHierarchy &&
                 Input.GetKeyDown(KeyCode.F))
        {
            NextLine();
        }
        else if (isInteracting && isTyping &&
                 Input.GetKeyDown(KeyCode.F))
        {
            FinishTyping();
        }
    }

    private void StartDialogue()
    {
        if (dialogueData == null ||
            dialogueData.dialogueLines == null ||
            dialogueData.dialogueLines.Length == 0)
        {
            Debug.LogWarning("Dialogue data or dialogue lines are missing.", this);
            return;
        }

        canInteract = false;
        isInteracting = true;

        DialogueBox.SetActive(true);
        NextLinePrompt.SetActive(false);

        ClearChoices();

        SyncQuestState();

        switch (questState)
        {
            case QuestState.NotStarted:
                lineIndex = dialogueData.questNotStartedIndex;
                break;

            case QuestState.InProgress:
                lineIndex = dialogueData.questInProgressIndex;
                break;

            case QuestState.Completed:
                lineIndex = dialogueData.questCompletedIndex;
                break;
        }

        if (!IsValidLineIndex(lineIndex))
        {
            EndDialogue();
            return;
        }

        ShowCurrentLine();
    }

    private void SyncQuestState()
    {
        if (dialogueData.quest == null)
        {
            questState = QuestState.NotStarted;
            return;
        }

        S_QuestManager questManager = FindObjectOfType<S_QuestManager>();
        if (questManager == null)
        {
            return;
        }

        string questID = dialogueData.quest.questID;

        var isQuestActiveMethod = questManager.GetType().GetMethod(
            "IsQuestActive",
            new[] { typeof(string) }
        );

        bool isQuestActive = isQuestActiveMethod != null &&
            isQuestActiveMethod.Invoke(questManager, new object[] { questID }) is bool active &&
            active;

        if (isQuestActive)
        {
            questState = QuestState.InProgress;
        }
        else
        {
            questState = QuestState.NotStarted;
        }
    }

    private void ShowCurrentLine()
    {
        if (!IsValidLineIndex(lineIndex))
        {
            EndDialogue();
            return;
        }

        StopDialogueCoroutines();
        ClearChoices();

        DialogueText.text = "";
        NextLinePrompt.SetActive(false);

        DialogueLine currentLine =
            dialogueData.dialogueLines[lineIndex];

        typingCoroutine = StartCoroutine(WriteLine(currentLine.text));
    }

    private IEnumerator WriteLine(string text)
    {
        isTyping = true;

        float typeSpeed = dialogueData.typingSpeed;

        foreach (char c in text)
        {
            DialogueText.text += c;

            if (typeSpeed > 0f)
                yield return new WaitForSeconds(typeSpeed);
            else
                yield return null;
        }

        isTyping = false;
        typingCoroutine = null;

        if (HasChoicesAtCurrentLine())
        {
            ShowChoices();
            yield break;
        }

        DialogueLine currentLine =
            dialogueData.dialogueLines[lineIndex];

        if (currentLine.endDialogue)
        {
            EndDialogue();
            yield break;
        }

        if (currentLine.autoProgress)
        {
            autoProgressCoroutine =
                StartCoroutine(AutoProgress());
        }
        else
        {
            NextLinePrompt.SetActive(true);
        }
    }

    private IEnumerator AutoProgress()
    {
        yield return new WaitForSeconds(
            dialogueData.autoProgressDelay
        );

        autoProgressCoroutine = null;

        if (isInteracting)
            NextLine();

    }

    private void FinishTyping()
    {
        if (typingCoroutine == null)
            return;

        StopCoroutine(typingCoroutine);
        typingCoroutine = null;

        DialogueText.text = dialogueData.dialogueLines[lineIndex].text;
        isTyping = false;

        DialogueLine currentLine = dialogueData.dialogueLines[lineIndex];

        if (HasChoicesAtCurrentLine())
        {
            ShowChoices();
        }
        else if (currentLine.endDialogue)
        {
            EndDialogue();
        }
        else if (currentLine.autoProgress)
        {
            autoProgressCoroutine = StartCoroutine(AutoProgress());
        }
        else
        {
            NextLinePrompt.SetActive(true);
        }
    }

    private void NextLine()
    {
        if (HasChoicesAtCurrentLine())
            return;

        StopDialogueCoroutines();

        DialogueLine currentLine = dialogueData.dialogueLines[lineIndex];

        if (currentLine.endDialogue)
        {
            EndDialogue();
            return;
        }

        if (lineIndex < dialogueData.dialogueLines.Length - 1)
        {
            lineIndex++;
            ShowCurrentLine();
        }
        else
        {
            EndDialogue();
        }
    }

    private bool HasChoicesAtCurrentLine()
    {
        if (dialogueData.choices == null)
            return false;

        foreach (DialogueChoice dialogueChoice in dialogueData.choices)
        {
            if (dialogueChoice != null &&
                dialogueChoice.dialogueIndex == lineIndex &&
                dialogueChoice.choices != null &&
                dialogueChoice.choices.Length > 0)
            {
                return true;
            }
        }

        return false;
    }

    private void ShowChoices()
    {
        ClearChoices();
        NextLinePrompt.SetActive(false);

        foreach (DialogueChoice dialogueChoice in dialogueData.choices)
        {
            if (dialogueChoice == null ||
                dialogueChoice.dialogueIndex != lineIndex ||
                dialogueChoice.choices == null)
            {
                continue;
            }

            foreach (Choice choice in dialogueChoice.choices)
            {
                if (choice == null)
                    continue;

                CreateChoiceButton(choice.text,() => SelectChoice(choice)
                );
            }

            break;
        }
    }

    private void SelectChoice(Choice choice)
    {
        if (choice == null)
            return;

        if (!IsValidLineIndex(choice.nextDialogueIndex))
        {
            Debug.LogWarning("Choice dialogue index is invalid.", this);
            return;
        }
        lineIndex = choice.nextDialogueIndex;
        ShowCurrentLine();
    }

    private bool IsValidLineIndex(int index)
    {
        return dialogueData != null &&
               dialogueData.dialogueLines != null &&
               index >= 0 &&
               index < dialogueData.dialogueLines.Length;
    }

    private void EndDialogue()
    {
        StopDialogueCoroutines();

        isTyping = false;
        isInteracting = false;

        if (DialogueBox != null)
            DialogueBox.SetActive(false);

        if (NextLinePrompt != null)
            NextLinePrompt.SetActive(false);

        ClearChoices();
        canInteract = IsPlayerInsideTrigger;
    }

    private bool IsPlayerInsideTrigger { get; set; }

    private void StopDialogueCoroutines()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (autoProgressCoroutine != null)
        {
            StopCoroutine(autoProgressCoroutine);
            autoProgressCoroutine = null;
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        IsPlayerInsideTrigger = true;

        if (!isInteracting)
            canInteract = true;
    }

    private void OnTriggerExit(Collider collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        IsPlayerInsideTrigger = false;
        canInteract = false;
    }

    public void ClearChoices()
    {
        if (choicesContainer == null)
            return;

        foreach (Transform child in choicesContainer)
            Destroy(child.gameObject);
    }

    public GameObject CreateChoiceButton(
        string choiceText,
        UnityEngine.Events.UnityAction onClickAction)
    {
        if (choiceButtonPrefab == null || choicesContainer == null)
        {
            Debug.LogWarning(
                "Choice button prefab or container is missing.",
                this
            );
            return null;
        }

        GameObject choiceButton =
            Instantiate(choiceButtonPrefab, choicesContainer);

        TMP_Text buttonText =
            choiceButton.GetComponentInChildren<TMP_Text>();

        if (buttonText != null)
            buttonText.text = choiceText;

        Button button = choiceButton.GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(onClickAction);

        return choiceButton;
    }
}