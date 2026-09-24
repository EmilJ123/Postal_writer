using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public enum MapTool
{
    Pen,
    Eraser,
    Sticker
}

public class MapGridController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RawImage mapRawImage;
    [SerializeField] private RectTransform mapRectTransform;

    [Header("Texture Settings")]
    [SerializeField] private int textureWidth = 512;
    [SerializeField] private int textureHeight = 512;
    [SerializeField] private Color canvasBackgroundColor = Color.white;

    [Header("Drawing Tools")]
    [SerializeField] private Color penColor = Color.black;
    [SerializeField] private int penRadius = 3;
    [SerializeField] private int eraserRadius = 10;

    private Texture2D mapTexture;
    private MapTool currentTool = MapTool.Pen;
    private Sprite selectedStickerSprite;
    private List<GameObject> placedStickers = new List<GameObject>();

    private void Awake()
    {
        InitializeTexture();
    }

    private void InitializeTexture()
    {
        mapTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        ClearTexture();
        mapRawImage.texture = mapTexture;
    }

    public void ClearTexture()
    {
        Color[] clearColors = new Color[textureWidth * textureHeight];
        for (int i = 0; i < clearColors.Length; i++)
        {
            clearColors[i] = canvasBackgroundColor;
        }
        mapTexture.SetPixels(clearColors);
        mapTexture.Apply();

        ClearAllStickers();
    }

    public void ClearAllStickers()
    {
        foreach (GameObject sticker in placedStickers)
        {
            Destroy(sticker);
        }
        placedStickers.Clear();
    }

    public void SetTool(MapTool tool)
    {
        currentTool = tool;
    }

    public void SelectSticker(Sprite stickerSprite)
    {
        selectedStickerSprite = stickerSprite;
        currentTool = MapTool.Sticker;
    }

    public void HandlePointerInput(Vector2 screenPoint)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mapRectTransform,
            screenPoint,
            null,
            out Vector2 localPoint
        );

        // Map local UI coordinates (centered) to Normalized UV Coordinates [0, 1]
        Vector2 rectSize = mapRectTransform.rect.size;
        float u = (localPoint.x + rectSize.x * 0.5f) / rectSize.x;
        float v = (localPoint.y + rectSize.y * 0.5f) / rectSize.y;

        if (u < 0f || u > 1f || v < 0f || v > 1f) return;

        int texX = Mathf.FloorToInt(u * textureWidth);
        int texY = Mathf.FloorToInt(v * textureHeight);

        switch (currentTool)
        {
            case MapTool.Pen:
                DrawCircle(texX, texY, penRadius, penColor);
                break;
            case MapTool.Eraser:
                DrawCircle(texX, texY, eraserRadius, canvasBackgroundColor);
                break;
            case MapTool.Sticker:
                PlaceStickerAtLocalPosition(localPoint);
                break;
        }
    }

    private void DrawCircle(int cx, int cy, int radius, Color color)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    int px = cx + x;
                    int py = cy + y;
                    if (px >= 0 && px < textureWidth && py >= 0 && py < textureHeight)
                    {
                        mapTexture.SetPixel(px, py, color);
                    }
                }
            }
        }
        mapTexture.Apply();
    }

    private void PlaceStickerAtLocalPosition(Vector2 localPosition)
    {
        if (selectedStickerSprite == null) return;

        GameObject newSticker = new GameObject("PlacedSticker", typeof(Image));
        newSticker.transform.SetParent(mapRectTransform, false);

        RectTransform stickerRect = newSticker.GetComponent<RectTransform>();
        stickerRect.anchoredPosition = localPosition;
        stickerRect.sizeDelta = new Vector2(40f, 40f);

        Image img = newSticker.GetComponent<Image>();
        img.sprite = selectedStickerSprite;
        img.raycastTarget = false;

        placedStickers.Add(newSticker);
    }
}