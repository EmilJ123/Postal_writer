
using System;
using UnityEngine;

[Serializable]
public class Choice
{
    public string text;
    public int nextDialogueIndex;

    public bool givesQuest;
    public S_QuestManager quest;
}

