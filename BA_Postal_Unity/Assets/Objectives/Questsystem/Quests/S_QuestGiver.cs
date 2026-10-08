using UnityEngine;

[CreateAssetMenu(fileName = "QuestGiver", menuName = "Quest Dialogue")]
public class S_QuestGiver : ScriptableObject
{
    public string[] dialogueLines;
    public bool[] autoProgressLines;
    public bool[] endDialogueLines;
    public float autoProgressDelay = 1.5f;
    public float typingSpeed = 0.05f;

    public DialogueChoice[] choices;

    public int questInProgressIndex;
    public int questCompletedIndex;
    public S_QuestManager quest;

    [System.Serializable]

    public class DialogueChoice
    {
        public int dialogueIndes;
        public string[] choices;
        public int[] nextDialogueIndexes;
        public bool[] givesQuests;
    }
}
