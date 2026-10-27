using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Rendering.MaterialUpgrader;

public class S_DialogueController : MonoBehaviour 
{
    [SerializeField] private GameObject InteractPromt;
    [SerializeField] private GameObject DialogueBox;
    [SerializeField] private TMP_Text DialogueText;
    [SerializeField] private GameObject NextLinePrompt;
    [SerializeField] private string[] DialogueLines;
    [SerializeField] private float TypeSpeed = 0.0f;




    private int lineIndex = 0;

    private bool isInteracting = false;
    private bool canInteract = false;
    public Transform choicesContainer;
    public GameObject choiceButtonPrefab;

    private void Update()
    {
        InteractPromt.SetActive(canInteract);

        if (canInteract && !isInteracting && Input.GetKeyDown(KeyCode.F))
            StartDialogue();

        if (isInteracting && NextLinePrompt.activeInHierarchy && Input.GetKeyDown(KeyCode.F))
            NextLine();
    }

    private void StartDialogue()
    {
        canInteract = false;
        isInteracting = true;

        DialogueText.text = "";
        NextLinePrompt.SetActive(false);
        DialogueBox.SetActive(true);

        lineIndex = 0;
        StartCoroutine(WriteLine());
    }

    private IEnumerator WriteLine()
    {
        foreach (char c in DialogueLines[lineIndex])
        {
            DialogueText.text += c;
            yield return new WaitForSeconds(TypeSpeed);
        }

        NextLinePrompt.SetActive(true);

    }

    private void NextLine()
    {
        if (lineIndex < DialogueLines.Length - 1)
        {
            DialogueText.text = "";
            NextLinePrompt.SetActive(false);

            lineIndex++;
            StartCoroutine(WriteLine());
        }
        else
        {
            DialogueBox.SetActive(false);
            NextLinePrompt.SetActive(false);

            isInteracting = false;
            canInteract = true;
        }

    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player") && !isInteracting)
            canInteract = true;
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.CompareTag("Player"))
            canInteract = false;
    }

    public void ClearChoices() 
    { 
        foreach (Transform child in choicesContainer) Destroy(child.gameObject);
    }

    public GameObject CreateChoiceButton(string choiceText, UnityEngine.Events.UnityAction onClickAction) 
    { 
        GameObject choiceButton = Instantiate(choiceButtonPrefab, choicesContainer); 
        TMP_Text buttonText = choiceButton.GetComponentInChildren<TMP_Text>(); 
        buttonText.text = choiceText; 

        Button button = choiceButton.GetComponent<Button>(); 
        button.onClick.AddListener(onClickAction); 

        return choiceButton; 
    }
}
