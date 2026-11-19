using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class S_ItemSlot : MonoBehaviour, IDropHandler
{
    public int slotIndex;
    
    public void OnDrop(PointerEventData eventData)
    {
        //Debug.Log("DROP DETECTED");

        if (eventData.pointerDrag != null)
        {
            eventData.pointerDrag.transform.SetParent(transform);

            eventData.pointerDrag.GetComponent<RectTransform>().localPosition =
                Vector2.zero;

            FindAnyObjectByType<S_PuzzleManager_M>().CheckAnswer();
        }
    }
    
}

