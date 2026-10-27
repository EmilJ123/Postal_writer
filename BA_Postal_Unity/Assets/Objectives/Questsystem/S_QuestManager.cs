using System;
using System.Collections; 
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quests/Quest")] 
public class S_QuestManager : ScriptableObject 
{ 
    public string questID; 
    public string questName; 
    public string description; 
    public List<QuestObjective> objectives; 

    private void OnValidate() 
    { 
        if (string.IsNullOrEmpty(questID)) 
        { 
            questID = questName + Guid.NewGuid().ToString();
        } 
    } 
}


 [System.Serializable] 
    public class QuestObjective 
    { 
        public string objectiveID; 
        public string description; 
        public ObjectiveType type; 
        public int requiredAmount; 
        public int currentAmount; 
        public bool IsCompleted => currentAmount >= requiredAmount; 
    } 

    public enum ObjectiveType { CollectItem, ReachLocation, TalkNPC, Custom } 

    [System.Serializable] 
    public class QuestProgress 
    { 
        public S_QuestManager quest; 
        public List<QuestObjective> objectives; 

        public QuestProgress(S_QuestManager quest) 
        { 
            this.quest = quest; 
            objectives = new List<QuestObjective>(); 
            
            foreach(var obj in objectives) 
            { 
                objectives.Add(new QuestObjective 
                { 
                    objectiveID = obj.objectiveID, 
                    description = obj.description, 
                    type = obj.type, 
                    requiredAmount = obj.requiredAmount, 
                    currentAmount = 0 
                }); 
            } 
        } 

        public bool IsCompleted => objectives.TrueForAll(o => o.IsCompleted); 
        public string QuestID => quest.questID;


} 
