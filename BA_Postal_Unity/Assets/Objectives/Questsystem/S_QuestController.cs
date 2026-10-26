using System.Collections.Generic;
using UnityEngine;

public class S_QuestController : MonoBehaviour
{
    public static S_QuestController Instance { get; private set; }

    public List<QuestProgress> activateQuests = new();
    private S_QuestUI questUI;

    public List<string> handinQuestIDs = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(GameObject);

        questUI = FindObjectOfType<S_QuestUI>();
        S_InventoryController.Instance.OnInventoryChanged += CheckInventoryForQuests;
       

    }

    public void AcceptQuest(S_QuestManager quest)
    {
        if (IsQuestActive(quest.questID)) return;

        activateQuests.Add(new QuestProgress(quest));

        CheckInventoryForQuests();
        questUI.UpdateQuestUI();
    }

    public bool IsQuestActive(string questID) => activateQuests.Exists(q => q.QuestID == questID);


    public void CheckInventoryForQuests()
    {
        Dictionary<int, int> itemCounts = S_InventoryController.Instance.GetItemCounts();

        foreach (QuestProgress quest in activateQuests)
        {
            if (questObjective.type != ObjectiveType.CollectItem) continue;
            if (!int.TryParse(questObjective.objectiveID, out int itemID)) continue;

            int newAmount = itemCounts.TryGetValue(itemID, out int count) ? Mathf.Min(count, questObjective.requiredAmount) : 0;
        }
    }

    public bool IsQuestCompleted(string questID)
    {
        QuestProgress quest = activateQuests.Find(q => q.QuestID == questID);
        return quest != null && quest.objectives.TrueForAll(o => o.IsCompleted);
    }

    public void HandInQuest(string questID)
    {
        //Try remove required items

        //remove quest from quest log
    }

    public void LoadQuestProgress(List<QuestProgress> savedQuests)
    {
        activateQuests = savedQuests ?? new();

        CheckInventoryForQuests();
        questUI.UpdateQuestUI();
    }
}