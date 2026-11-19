using UnityEngine;
using UnityEngine.EventSystems;
public class DragDrop : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
{
    [SerializeField] private Canvas canvas;

    public string word;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        //Debug.Log("OnBeginDrag");
        originalParent = transform.parent;
        canvasGroup.alpha = .6f;
        canvasGroup.blocksRaycasts = false;
        
    }

    public void OnDrag(PointerEventData eventData)
    {
        //Debug.Log("OnDrag");
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        //Debug.Log("OnEndDrag");
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (transform.parent == originalParent)
        {
            rectTransform.localPosition = Vector3.zero;
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
       // Debug.Log("OnPointerDown");
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        Transform draggedWord = eventData.pointerDrag.transform;

        // Does this slot already have a word?
        DragDrop existingWord = GetComponentInChildren<DragDrop>();

        if (existingWord != null)
        {
            Transform oldParent = draggedWord.parent;

            existingWord.transform.SetParent(oldParent);
            existingWord.GetComponent<RectTransform>().localPosition = Vector2.zero;
        }

        draggedWord.SetParent(transform);
        draggedWord.GetComponent<RectTransform>().localPosition = Vector2.zero;

        FindAnyObjectByType<S_PuzzleManager_M>().CheckAnswer();
    }
}
