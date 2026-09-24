using System;
using System.Collections; // Kan fjernes hvis den ikke brukes, rettet skrivefeil "Sustem"
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quests/Quest")] // Lagt til manglende anførselstegn rundt meny-stien
public class S_QuestManager : ScriptableObject 
{ 
    public string questID; 
    public string questName; 
    [TextArea(3, 10)] public string description; // La til TextArea for bedre visning i Inspektøren
    public List<QuestObjective> objectives; 

    private void OnValidate() 
    { 
        if (string.IsNullOrEmpty(questID)) 
        { 
            // Endret Guid-generering for å unngå potensielle ulovlige filnavn/tegn i ID-en
            questID = questName + "_" + Guid.NewGuid().ToString("N"); 
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
        // Endret type fra 'Quest' til 'S_QuestManager' siden klassen din heter S_QuestManager
        public S_QuestManager quest; 
        public List<QuestObjective> objectives; 

        public QuestProgress(S_QuestManager quest) 
        { 
            this.quest = quest; 
            objectives = new List<QuestObjective>(); 
            
            // RETTET LOGISK FEIL: Slår opp i 'quest.objectives' i stedet for den tomme lokale listen
            foreach(var obj in quest.objectives) 
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
}
