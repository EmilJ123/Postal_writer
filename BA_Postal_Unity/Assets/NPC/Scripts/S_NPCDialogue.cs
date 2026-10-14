using System;
using UnityEngine;


[CreateAssetMenu(fileName = "New NPC Dialogue", menuName = "NPC Dialogue")]
public class S_NPCDialogue : ScriptableObject
{
       public string npcName;
    public Sprite npcPortrait;

    public DialogueLine[] dialogueLines;

    public float autoProgressDelay = 1.5f;
    public float typingSpeed = 0.05f;

    public AudioClip voiceSound;
    public float voicePitch = 1.0f;

    public DialogueChoice[] choices;
    public bool[] endDialogueLines;
}

[System.Serializable]

public class DialogueLine
{
    [TextArea(2, 5)]
    public string text;

    public bool autoProgress;

    public bool endDialogue;
}

[Serializable]
public class DialogueChoice
{
    public int dialogueIndex; //Dialogue line where choices appear
    public string[] choices; //Player response options
    public int[] nextDialogueIndices; //Where choice leads
}
