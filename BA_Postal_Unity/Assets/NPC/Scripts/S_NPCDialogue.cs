using System;
using UnityEngine;

[CreateAssetMenu(
    fileName = "New NPC Dialogue",
    menuName = "NPC Dialogue"
)]
public class S_NPCDialogue : ScriptableObject
{
    [Header("NPC Information")]
    public string npcName;
    public Sprite npcPortrait;


    [Header("Dialogue")]
    public DialogueLine[] dialogueLines;


    [Header("Typing")]
    [Min(0f)]
    public float typingSpeed = 0.05f;

    [Min(0f)]
    public float autoProgressDelay = 1.5f;


    [Header("Voice")]
    public AudioClip voiceSound;

    [Min(0f)]
    public float voicePitch = 1.0f;


    [Header("Dialogue Choices")]
    public DialogueChoice[] choices;
}


[Serializable]
public class DialogueLine
{
    [TextArea(2, 5)]
    public string text;

    [Tooltip("Automatically continue this dialogue line.")]
    public bool autoProgress;

    [Tooltip("End the dialogue after this line.")]
    public bool endDialogue;
}




[Serializable]
public class DialogueChoice
{
    [Tooltip("The dialogue line where these choices appear.")]
    public int dialogueIndex;

    [Tooltip("The choices the player can select.")]
    public Choice[] choices;
}


[Serializable]
public class Choice
{
    [Tooltip("Text displayed on the choice button.")]
    public string text;

    [Tooltip("Dialogue line to go to after selecting this choice.")]
    public int nextDialogueIndex;

    [Header("Quest")]

    [Tooltip("Does this choice give a quest?")]
    public bool givesQuest;

    [Tooltip("Quest given by this choice.")]
    public S_QuestManager quest;
}