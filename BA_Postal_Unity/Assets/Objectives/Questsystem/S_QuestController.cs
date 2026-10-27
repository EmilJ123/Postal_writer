using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class S_QuestController : MonoBehaviour
{
    public static S_QuestController Instance { get; private set; }
    public List<QuestProgress> activateQuests = new();
    private S_QuestUI questUI;

    [System.Obsolete]
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        questUI = FindObjectOfType<S_QuestUI>();
    }

    public void AcceptQuest(S_QuestManager quest)
    {
        if (IsQuestActive(quest.questID)) return;

        activateQuests.Add(new QuestProgress(quest));

        questUI.UpdateQuestUI();
    }

    public bool IsQuestActive(string questID) => activateQuests.Exists(q => q.QuestID == questID);
}
