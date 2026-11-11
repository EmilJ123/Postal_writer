
using System.Collections.Generic;
using UnityEngine;

public class S_QuestController : MonoBehaviour
{
    public static S_QuestController Instance { get; private set; }
    public List<QuestProgress> activateQuests = new();
    public S_QuestUI questUI;

    private void Awake()
    {
        Instance = this;

        questUI = GetComponent<S_QuestUI>();

        if (questUI == null)
        {
            Debug.LogError("Could not find S_QuestUI!");
        }
    }
    
    public void AcceptQuest(S_QuestManager quest)
    {
        Debug.Log("AcceptQuest called!");

        if (IsQuestActive(quest.questID)) return;

        activateQuests.Add(new QuestProgress(quest));

        Debug.Log("Quest added: " + quest.questName);

        questUI.UpdateQuestUI();
    }

    public bool IsQuestActive(string questID)
        => activateQuests.Exists(q => q.QuestID == questID);
}
