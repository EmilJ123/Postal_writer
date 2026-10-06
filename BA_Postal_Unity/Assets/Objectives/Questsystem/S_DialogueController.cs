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
    public Image portraitImage; 

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

    public void SetNPCInfo(string npcName, Sprite npcPortrait) 
    { 
        nameText.text = npcName; 
        portraitImage.sprite = npcPortrait; 
    } 

    public void SetDialogueText(string text) 
    { 
        dialogueText.text = text; 
    } 
}
