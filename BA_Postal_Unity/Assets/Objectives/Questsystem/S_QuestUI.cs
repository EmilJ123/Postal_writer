using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class S_QuestUI : MonoBehaviour
{
    public Transform questListContent;
    public GameObject questEntryPrefab;
    public GameObject objectiveTextPrefab;

    //public S_QuestManager testQuest;
    //public int testQuestAmount;

    //private List<QuestProgress> testQuests = new();

    void Start()
    {
        //for (int i = 0; i < testQuestAmount; i++)
        {
            //testQuests.Add(new QuestProgress(testQuest));
        }
        
        UpdateQuestUI();
    }

    public void UpdateQuestUI()
    {
        // Destroy existing quest entries
        foreach (Transform child in questListContent)
        {
            Destroy(child.gameObject);
        }
        var manager = FindAnyObjectByType<S_QuestManager>();
        
        // Create new quest entries
        foreach(var quest in S_QuestController.Instance.activateQuests) //no reference no clue as to why i'll fix it later i'm tired af atm
        {
            GameObject entry = Instantiate(questEntryPrefab, questListContent);
            TMP_Text questNameText = entry.transform.Find("QuestName").GetComponent<TMP_Text>();
            
            Transform objectiveList = entry.transform.Find("ObjectiveList");

            questNameText.text = quest.quest.name; 

            foreach (var objective in quest.objectives)
            {
                GameObject objTextGo = Instantiate(objectiveTextPrefab, objectiveList);
                
                TMP_Text objText = objTextGo.GetComponent<TMP_Text>();
               
                objText.text = $"{objective.description} ({objective.currentAmount}/{objective.requiredAmount})";
            }
        }
    }
}
