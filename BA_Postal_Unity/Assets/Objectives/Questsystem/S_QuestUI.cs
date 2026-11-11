using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class S_QuestUI : MonoBehaviour
{
    public Transform questListContent;
    public GameObject questEntryPrefab;
    public GameObject objectiveTextPrefab;
    

    void Start()
    {
        UpdateQuestUI();
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void UpdateQuestUI()
    {
        // Destroy existing quest entries
        foreach (Transform child in questListContent)
        {
            Destroy(child.gameObject);
        }
        var manager = FindAnyObjectByType<S_QuestManager>(); //isn't being used for some reason
        
        // Create New Quest Entries
        foreach(var quest in S_QuestController.Instance.activateQuests) //no reference no clue as to why i'll fix it later i'm tired af atm
        {
            GameObject entry = Instantiate(questEntryPrefab, questListContent);
            TMP_Text questNameText = entry.transform.Find("QuestName").GetComponent<TMP_Text>();
            
            Transform objectiveList = entry.transform.Find("ObjectiveList");

            questNameText.text = quest.quest.name; //this had an issue??? 

            foreach (var objective in quest.objectives)
            {
                GameObject objTextGo = Instantiate(objectiveTextPrefab, objectiveList);
                
                TMP_Text objText = objTextGo.GetComponent<TMP_Text>();
               
                objText.text = $"{objective.description} ({objective.currentAmount}/{objective.requiredAmount})";
            }
        }
    }
}
