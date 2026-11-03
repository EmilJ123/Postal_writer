using UnityEngine;
using UnityEngine.UI;

public class TabController : MonoBehaviour
{

    public Image[] tabImages;
    public GameObject[] pages;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ActivateTab(0);
    }

    // Update is called once per frame
    public void ActivateTab(int tabNo)
    {
        Debug.Log("Clicked tab " + tabNo);
        Debug.Log("Activating tab: " + tabNo);
        for(int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(false);
            Debug.Log("Disabling: " + pages[i].name);
            tabImages[i].color = Color.grey;
        }
        pages[tabNo].SetActive(true);
        tabImages[tabNo].color = Color.white;
    }
}
