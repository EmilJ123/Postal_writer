using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;

public class S_QuestUI : MonoBehaviour
{
    public Transform questListContent;
    public GameObject questEntryPrefab;
    public GameObject objectiveTextPrefab;

    public S_QuestManager testQuest;
    public int testQuestAmount;

    private List<QuestProgress> testQuests = new();

    void Start()
    {
        for (int i = 0; i < testQuestAmount; i++)
        {
            testQuests.Add(new QuestProgress(testQuest));
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

        // Create new quest entries
        foreach (var questProgress in testQuests)
        {
            GameObject entry = Instantiate(questEntryPrefab, questListContent);
            TMP_Text questNameText = entry.transform.Find("QuestNameText").GetComponent<TMP_Text>();
            Transform objectiveList = entry.transform.Find("ObjectiveList");

            questNameText.text = questProgress.quest.name; 

            foreach (var objective in questProgress.objectives)
            {
                GameObject objTextGO = Instantiate(objectiveTextPrefab, objectiveList);
                TMP_Text objText = objTextGO.GetComponent<TMP_Text>();
                objText.text = $"{objective.description} ({objective.currentAmount}/{objective.requiredAmount})";
            }
        }
    }
}
