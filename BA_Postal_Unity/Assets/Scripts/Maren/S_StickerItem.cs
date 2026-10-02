using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class S_StickerItem : MonoBehaviour
{
    [SerializeField] private Sprite stickerSprite;
    [SerializeField] private S_MapGridController gridController;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnStickerSelected);
    }

    private void OnStickerSelected()
    {
        if (gridController != null && stickerSprite != null)
        {
            gridController.SelectSticker(stickerSprite);
        }
    }
}