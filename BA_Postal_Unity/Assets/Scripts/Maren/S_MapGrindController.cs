using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public enum MapTool
{
    Pen,
    Eraser,
    Sticker
}

public class S_MapGridController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RawImage mapRawImage;
    [SerializeField] private RectTransform mapRectTransform;

    [Header("Texture Settings")]
    [SerializeField] private int textureWidth = 512;
    [SerializeField] private int textureHeight = 512;
    [SerializeField] private Color canvasBackgroundColor = Color.white;

    [Header("Drawing Tools")]
    [SerializeField] private Color activePenColor = Color.black;
    [SerializeField] private int penRadius = 4;
    [SerializeField] private int eraserRadius = 12;

    [Header("Pen Stabilization")]
    [Tooltip("Higher values = smoother, less jittery lines (e.g., 0.15 to 0.4). 0 = disabled.")]
    [Range(0f, 0.8f)]
    [SerializeField] private float lineStabilization = 0.25f;

    private Texture2D mapTexture;
    private MapTool currentTool = MapTool.Pen;
    private Sprite selectedStickerSprite;
    private List<GameObject> placedStickers = new List<GameObject>();

    // Tracks current stroke positions for stabilization & interpolation
    private Vector2? stabilizedLocalPos = null;
    private Vector2Int? lastPixelPos = null;

    private void Awake()
    {
        if (mapRawImage == null) mapRawImage = GetComponent<RawImage>();
        if (mapRectTransform == null) mapRectTransform = GetComponent<RectTransform>();

        InitializeTexture();
    }

    private void InitializeTexture()
    {
        mapTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        ClearTexture();

        if (mapRawImage != null)
        {
            mapRawImage.texture = mapTexture;
        }
    }

    public void ClearTexture()
    {
        if (mapTexture == null) return;

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
        for (int i = placedStickers.Count - 1; i >= 0; i--)
        {
            if (placedStickers[i] != null)
            {
                Destroy(placedStickers[i]);
            }
        }
        placedStickers.Clear();
    }

    // --- COLOR & TOOL CONTROLS ---

    public void SetPenColor(Color newColor)
    {
        activePenColor = newColor;
        currentTool = MapTool.Pen;
    }

    // Call this from UI buttons (e.g., Red Button passes Hex/Color)
    public void SetPenColorHTML(string hexColor)
    {
        if (ColorUtility.TryParseHtmlString(hexColor, out Color parsedColor))
        {
            SetPenColor(parsedColor);
        }
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

    // --- EVENT TRIGGER INPUTS ---

    public void OnPointerDown(BaseEventData data)
    {
        PointerEventData pointerData = (PointerEventData)data;

        // Reset stroke smoothing on every click
        lastPixelPos = null;
        stabilizedLocalPos = null;

        if (currentTool == MapTool.Sticker && pointerData.button == PointerEventData.InputButton.Left)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mapRectTransform, pointerData.position, pointerData.pressEventCamera, out Vector2 localPoint))
            {
                PlaceStickerAtLocalPosition(localPoint);
            }
        }
        else
        {
            HandlePointerInput(data);
        }
    }

    public void HandlePointerInput(BaseEventData data)
    {
        if (currentTool == MapTool.Sticker) return;

        PointerEventData pointerData = (PointerEventData)data;
        if (mapRectTransform == null || mapTexture == null) return;

        bool isInside = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mapRectTransform,
            pointerData.position,
            pointerData.pressEventCamera,
            out Vector2 rawLocalPoint
        );

        if (!isInside)
        {
            lastPixelPos = null;
            stabilizedLocalPos = null;
            return;
        }

        // Apply Stabilization (Exponential Smoothing / Lazy Mouse Filter)
        Vector2 targetLocalPos;
        if (!stabilizedLocalPos.HasValue || lineStabilization <= 0f)
        {
            targetLocalPos = rawLocalPoint;
        }
        else
        {
            float lerpFactor = 1f - lineStabilization;
            targetLocalPos = Vector2.Lerp(stabilizedLocalPos.Value, rawLocalPoint, lerpFactor);
        }
        stabilizedLocalPos = targetLocalPos;

        // Map Local Coordinates to UV Pixels
        Vector2 rectSize = mapRectTransform.rect.size;
        float u = (targetLocalPos.x + rectSize.x * 0.5f) / rectSize.x;
        float v = (targetLocalPos.y + rectSize.y * 0.5f) / rectSize.y;

        if (u < 0f || u > 1f || v < 0f || v > 1f)
        {
            lastPixelPos = null;
            return;
        }

        int texX = Mathf.FloorToInt(u * textureWidth);
        int texY = Mathf.FloorToInt(v * textureHeight);
        Vector2Int currentPixelPos = new Vector2Int(texX, texY);

        switch (currentTool)
        {
            case MapTool.Pen:
                DrawSmoothLine(currentPixelPos, penRadius, activePenColor);
                break;

            case MapTool.Eraser:
                DrawSmoothLine(currentPixelPos, eraserRadius, canvasBackgroundColor);
                break;
        }
    }

    public void OnPointerUp(BaseEventData data)
    {
        lastPixelPos = null;
        stabilizedLocalPos = null;
    }

    // --- DRAWING ALGORITHMS ---

    private void DrawSmoothLine(Vector2Int targetPos, int radius, Color color)
    {
        if (lastPixelPos.HasValue && lastPixelPos.Value != targetPos)
        {
            Vector2Int start = lastPixelPos.Value;
            float distance = Vector2.Distance(start, targetPos);
            int steps = Mathf.Max(Mathf.CeilToInt(distance * 2f), 1);

            for (int i = 0; i <= steps; i++)
            {
                float t = (float)i / steps;
                int x = Mathf.RoundToInt(Mathf.Lerp(start.x, targetPos.x, t));
                int y = Mathf.RoundToInt(Mathf.Lerp(start.y, targetPos.y, t));
                DrawCircle(x, y, radius, color);
            }
        }
        else
        {
            DrawCircle(targetPos.x, targetPos.y, radius, color);
        }

        lastPixelPos = targetPos;
        mapTexture.Apply();
    }

    private void DrawCircle(int cx, int cy, int radius, Color color)
    {
        int rSquared = radius * radius;
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x * x + y * y <= rSquared)
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
    }

    private void PlaceStickerAtLocalPosition(Vector2 localPosition)
    {
        if (selectedStickerSprite == null) return;

        GameObject newSticker = new GameObject("PlacedSticker", typeof(Image));
        newSticker.transform.SetParent(mapRectTransform, false);

        RectTransform stickerRect = newSticker.GetComponent<RectTransform>();
        stickerRect.anchoredPosition = localPosition;
        stickerRect.sizeDelta = new Vector2(32f, 32f);

        Image img = newSticker.GetComponent<Image>();
        img.sprite = selectedStickerSprite;
        img.raycastTarget = false;

        placedStickers.Add(newSticker);
    }
}