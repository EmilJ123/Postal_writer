
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

public class S_QuestController : MonoBehaviour
{
    public static S_QuestController Instance { get; private set; }
    public List<QuestProgress> activateQuests = new();
    public S_QuestUI questUI;

    public List<string> handingQuestIDs = new();
    private void Awake()
    {
        Instance = this;

        questUI = GetComponent<S_QuestUI>();
        S_InventoryController.Instance.OnInventoryChanged += CheckInventoryForQuest;

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
        
        CheckInventoryForQuest();
        questUI.UpdateQuestUI();
    }

    public bool IsQuestActive(string questID)
        => activateQuests.Exists(q => q.QuestID == questID);

    public void CheckInventoryForQuest()
    {
        Dictionary<int, int> itemCounts = S_InventoryController.Instance.GetItemCounts();

        foreach (QuestProgress quest in activateQuests)
        {
            foreach (QuestObjective questObjective in quest.objectives)
            {
                if (questObjective.type != ObjectiveType.CollectItem) continue;
                if (!int.TryParse(questObjective.objectiveID, out int itemID)) continue;

                int newAmount = itemCounts.TryGetValue(itemID, out int count)
                    ? Mathf.Min(count, questObjective.requiredAmount)
                    : 0;
                if (questObjective.currentAmount != newAmount)
                {
                    questObjective.currentAmount = newAmount;
                }
            }
        }
        questUI.UpdateQuestUI();
    }
    public bool IsQuestCompleted(string questID)
     {
    QuestProgress quest = activateQuests.Find(q => q.QuestID == questID);
    return quest != null && quest.objectives.TrueForAll(o => o.IsCompleted);
    
     }

    public void HandInQuest(string questID)
    {
        //try remove required items
       if (!RemoveRequiredItemFromInventory(questID))
       {
           return;
       }
        //Remove quest from quest log
        QuestProgress quest = activateQuests.Find(q => q.QuestID == questID);
        if (quest != null)
        {
            handingQuestIDs.Add(questID);
            activateQuests.Remove(quest);
            questUI.UpdateQuestUI();
        }
    }

    public bool IsQuestHandedIn(string questID)
    {
        return handingQuestIDs.Contains(questID);
    }
    public bool RemoveRequiredItemFromInventory(string questID)
    {
        QuestProgress quest = activateQuests.Find(q => q.QuestID == questID);
        if (quest == null) return false;

        Dictionary<int, int> requiredItems = new();
        
        //item requirements from objectives
        foreach (QuestObjective objective in quest.objectives)
        {
            if (objective.type == ObjectiveType.CollectItem && int.TryParse(objective.objectiveID, out int itemID))
            {
                requiredItems[itemID] = objective.requiredAmount;
            }
        }
        //verify we have items
        Dictionary<int, int> itemCounts = S_InventoryController.Instance.GetItemCounts();
        foreach (var item in requiredItems)
        {
            if (itemCounts.GetValueOrDefault(item.Key) < item.Value)
            {
                return false;
            }
        }
        //remove requied items from iventory
        foreach (var itemRequirement in requiredItems)
        {
            //removeItemsFromInventory
            S_InventoryController.Instance.RemoveItemsFromInventory(itemRequirement.Key, itemRequirement.Value);
        }
        return true;
    }
    public void LoadQuestProgress(List<QuestProgress> savedQuests)
    {
        activateQuests = savedQuests ?? new();

        CheckInventoryForQuest();
        questUI.UpdateQuestUI();
    }
}
