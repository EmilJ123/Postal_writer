using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class S_DialogueController : MonoBehaviour 
{ 
    public static S_DialogueController instance { get; private set; } 
    
    public GameObject dialoguePanel; 
    public TMP_Text dialogueText;
    public TMP_Text nameText; 
    public Transform choicesContainer;
    public GameObject choiceButtonPrefab;

    void Awake() 
    { 
        if (instance == null) 
        {
            instance = this; 
        }
        else 
        {
            Destroy(gameObject); 
        }
    } 

    public void ShowDialogueUI(bool show) 
    { 
        dialoguePanel.SetActive(show); 
    } 

    public void SetNPCInfo(string npcName) 
    { 
        nameText.text = npcName; 
     
    } 

    public void SetDialogueText(string text) 
    { 
        dialogueText.text = text; 
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
