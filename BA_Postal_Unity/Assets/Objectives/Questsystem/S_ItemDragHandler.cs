using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    Transform originalParent;
    CanvasGroup canvasGroup;

    private S_InventoryController inventoryController;

    // Start is called before the first frame update
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        inventoryController = GetInstance();
    }

    private static S_InventoryController GetInstance()
    {
        return S_InventoryController.Instance;
    }

    private void ResetItemPosition()
    {
        RectTransform rect = GetComponent<RectTransform>();

        if (rect != null)
        {
            rect.anchoredPosition = Vector2.zero;
        }
        else
        {
            transform.localPosition = Vector3.zero;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent; //Save OG parent
        transform.SetParent(transform.root); //Above other canvas'

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.6f; //Semi-transparent during drag
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransform rect = GetComponent<RectTransform>();

        if (rect != null)
        {
            transform.position = eventData.position; //Follow the mouse
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true; //Enables raycasts
            canvasGroup.alpha = 1f; //No longer transparent
        }

        S_Slot dropSlot = eventData.pointerEnter?.GetComponent<S_Slot>(); //Slot where item dropped

        if (dropSlot == null)
        {
            GameObject dropItem = eventData.pointerEnter;

            if (dropItem != null)
            {
                dropSlot = dropItem.GetComponentInParent<S_Slot>();
            }
        }

        S_Slot originalSlot = originalParent != null
            ? originalParent.GetComponent<S_Slot>()
            : null;

        if (dropSlot == originalSlot)
        {
            transform.SetParent(originalParent);
            ResetItemPosition();
            return;
        }

        if (dropSlot != null && originalSlot != null)
        {
            if (dropSlot.currentItem != null)
            {
                S_Item draggedItem = GetComponent<S_Item>();
                S_Item targetItem = dropSlot.currentItem.GetComponent<S_Item>();

                if (draggedItem != null && targetItem != null && draggedItem.ID == targetItem.ID)
                {
                    targetItem.AddToStack(draggedItem.quantity);
                    originalSlot.currentItem = null;
                    Destroy(gameObject);
                }
                else
                {
                    //Slot has an item - swap items
                    GameObject targetObject = dropSlot.currentItem;

                    targetObject.transform.SetParent(originalSlot.transform);
                    originalSlot.currentItem = targetObject;

                    RectTransform targetRect = targetObject.GetComponent<RectTransform>();

                    if (targetRect != null)
                    {
                        targetRect.anchoredPosition = Vector2.zero;
                    }
                    else
                    {
                        targetObject.transform.localPosition = Vector3.zero;
                    }

                    transform.SetParent(dropSlot.transform);
                    dropSlot.currentItem = gameObject;
                    ResetItemPosition();
                }
            }
            else
            {
                originalSlot.currentItem = null;
                transform.SetParent(dropSlot.transform);
                dropSlot.currentItem = gameObject;
                ResetItemPosition();
            }

            if (inventoryController != null)
            {
                inventoryController.RebuildItemCounts();
            }
        }
        else
        {
            transform.SetParent(originalParent);
            ResetItemPosition();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            SplitStack();
        }
    }

    private void SplitStack()
    {
        S_Item item = GetComponent<S_Item>();
        if (item == null || item.quantity <= 1) return;

        int splitAmount = item.quantity / 2;
        if (splitAmount <= 0) return;

        if (inventoryController == null) return;

        GameObject newItem = item.CloneItem(splitAmount);

        if (newItem == null) return;

        foreach (Transform slotTransform in inventoryController.inventoryPanel.transform)
        {
            S_Slot slot = slotTransform.GetComponent<S_Slot>();

            if (slot != null && slot.currentItem == null)
            {
                item.RemoveFromStack(splitAmount);

                newItem.transform.SetParent(slot.transform);

                RectTransform rect = newItem.GetComponent<RectTransform>();

                if (rect != null)
                {
                    rect.anchoredPosition = Vector2.zero;
                }
                else
                {
                    newItem.transform.localPosition = Vector3.zero;
                }

                slot.currentItem = newItem;
                inventoryController.RebuildItemCounts();
                return;
            }
        }

        Destroy(newItem);
    }
}