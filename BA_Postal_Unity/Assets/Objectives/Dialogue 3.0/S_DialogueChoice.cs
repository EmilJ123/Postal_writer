using UnityEngine;

[System.Serializable]
public class S_DialogueChoice : MonoBehaviour
{
    public int dialogueIndex;

    public string[] choices;

    public int[] nextDialogueIndexes;
}