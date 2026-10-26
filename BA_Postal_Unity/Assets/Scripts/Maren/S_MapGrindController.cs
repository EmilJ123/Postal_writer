using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
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
    [SerializeField] private Color activePenColor = Color.yellow;
    [SerializeField] private Color activeOutlineColor = new Color(1f, 0.5f, 0f);
    [SerializeField] private float penRadius = 5f;
    [SerializeField] private float outlineThickness = 1.2f;
    [SerializeField] private float eraserRadius = 12f;

    [Header("Preset Brush Sizes")]
    [SerializeField] private float smallSize = 3f;
    [SerializeField] private float mediumSize = 7f;
    [SerializeField] private float largeSize = 12f;

    [Header("Stroke Averaging & Smoothing")]
    [Tooltip("Minimum pixel distance required before registering a new curve control point.")]
    [SerializeField] private float minPointDistance = 4f;
    [Tooltip("How aggressively sharp corners/fast drags are smoothed into rounded curves (higher = rounder curves).")]
    [Range(0f, 1f)]
    [SerializeField] private float curveAveraging = 0.5f;

    private Texture2D mapTexture;
    private MapTool currentTool = MapTool.Pen;
    private Sprite selectedStickerSprite;
    private List<GameObject> placedStickers = new List<GameObject>();

    // Input & Curve Control Buffers
    private Coroutine drawCoroutine = null;
    private List<Vector2> rawControlPoints = new List<Vector2>();
    private Vector2 currentMouseScreenPos;
    private Camera currentPointerCamera;
    private Vector2? lastEvaluatedSplinePos = null;
    private bool isDrawing = false;

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

    // --- CONTROLS ---

    public void SetPenRadius(float newRadius) => penRadius = Mathf.Max(0.5f, newRadius);

    public void SetPresetSize(int presetIndex)
    {
        switch (presetIndex)
        {
            case 0: penRadius = smallSize; break;
            case 1: penRadius = mediumSize; break;
            case 2: penRadius = largeSize; break;
        }
    }

    public void SetPenColorWithOutline(Color mainColor, Color outlineColor)
    {
        activePenColor = mainColor;
        activeOutlineColor = outlineColor;
        currentTool = MapTool.Pen;
    }

    public void SetTool(MapTool tool) => currentTool = tool;

    public void SelectSticker(Sprite stickerSprite)
    {
        selectedStickerSprite = stickerSprite;
        currentTool = MapTool.Sticker;
    }

    // --- INPUT POINTER EVENTS ---

    public void OnPointerDown(BaseEventData data)
    {
        PointerEventData pointerData = (PointerEventData)data;

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
            isDrawing = true;
            rawControlPoints.Clear();
            lastEvaluatedSplinePos = null;

            currentMouseScreenPos = pointerData.position;
            currentPointerCamera = pointerData.pressEventCamera;

            // Seed initial stroke position
            if (TryGetTexturePixel(currentMouseScreenPos, currentPointerCamera, out Vector2 pixelPos))
            {
                rawControlPoints.Add(pixelPos);
            }

            if (drawCoroutine != null) StopCoroutine(drawCoroutine);
            drawCoroutine = StartCoroutine(DrawLoopRoutine());
        }
    }

    public void HandlePointerInput(BaseEventData data)
    {
        PointerEventData pointerData = (PointerEventData)data;
        currentMouseScreenPos = pointerData.position;
        currentPointerCamera = pointerData.pressEventCamera;
    }

    public void OnPointerUp(BaseEventData data)
    {
        isDrawing = false;
        if (drawCoroutine != null)
        {
            StopCoroutine(drawCoroutine);
            drawCoroutine = null;
        }

        // Finalize remaining curve segments on lift
        if (rawControlPoints.Count > 1)
        {
            FlushRemainingSplineSegments();
        }

        if (mapTexture != null)
        {
            mapTexture.Apply();
        }

        rawControlPoints.Clear();
        lastEvaluatedSplinePos = null;
    }

    // --- DRAWING COROUTINE & SPLINE EVALUATION ---

    private IEnumerator DrawLoopRoutine()
    {
        WaitForFixedUpdate fixedWait = new WaitForFixedUpdate();

        while (isDrawing)
        {
            if (TryGetTexturePixel(currentMouseScreenPos, currentPointerCamera, out Vector2 currentPixel))
            {
                if (rawControlPoints.Count == 0)
                {
                    rawControlPoints.Add(currentPixel);
                }
                else
                {
                    Vector2 lastAdded = rawControlPoints[rawControlPoints.Count - 1];
                    if (Vector2.Distance(lastAdded, currentPixel) >= minPointDistance)
                    {
                        rawControlPoints.Add(currentPixel);
                        EvaluateAndDrawSplineSegment();
                    }
                }
            }

            yield return fixedWait;
        }
    }

    private void EvaluateAndDrawSplineSegment()
    {
        int count = rawControlPoints.Count;
        if (count < 2) return;

        // Catmull-Rom Spline point evaluation
        Vector2 p0 = (count >= 3) ? rawControlPoints[count - 3] : rawControlPoints[count - 2];
        Vector2 p1 = rawControlPoints[count - 2];
        Vector2 p2 = rawControlPoints[count - 1];
        Vector2 p3 = (count >= 4) ? rawControlPoints[count - 1] : p2;

        float distance = Vector2.Distance(p1, p2);
        int steps = Mathf.Max(Mathf.CeilToInt(distance * 2f), 4);

        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            Vector2 splinePoint = CalculateCatmullRom(p0, p1, p2, p3, t, curveAveraging);

            if (lastEvaluatedSplinePos.HasValue)
            {
                RenderLineSegment(lastEvaluatedSplinePos.Value, splinePoint);
            }
            lastEvaluatedSplinePos = splinePoint;
        }

        mapTexture.Apply();
    }

    private void FlushRemainingSplineSegments()
    {
        int count = rawControlPoints.Count;
        if (count < 2) return;

        Vector2 p0 = (count >= 3) ? rawControlPoints[count - 3] : rawControlPoints[count - 2];
        Vector2 p1 = rawControlPoints[count - 2];
        Vector2 p2 = rawControlPoints[count - 1];

        float distance = Vector2.Distance(p1, p2);
        int steps = Mathf.Max(Mathf.CeilToInt(distance * 2f), 4);

        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            Vector2 splinePoint = CalculateCatmullRom(p0, p1, p2, p2, t, curveAveraging);

            if (lastEvaluatedSplinePos.HasValue)
            {
                RenderLineSegment(lastEvaluatedSplinePos.Value, splinePoint);
            }
            lastEvaluatedSplinePos = splinePoint;
        }
    }

    // Centripetal Catmull-Rom Spline Formula
    private Vector2 CalculateCatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t, float alpha)
    {
        Vector2 a = 2f * p1;
        Vector2 b = p2 - p0;
        Vector2 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
        Vector2 d = -p0 + 3f * p1 - 3f * p2 + p3;

        return 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));
    }

    // --- PIXEL RENDERING ENGINE ---

    private void RenderLineSegment(Vector2 startPos, Vector2 endPos)
    {
        float dist = Vector2.Distance(startPos, endPos);
        int steps = Mathf.Max(Mathf.CeilToInt(dist * 2f), 1);

        List<Vector2Int> pixelPath = new List<Vector2Int>();
        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            Vector2 p = Vector2.Lerp(startPos, endPos, t);
            Vector2Int pixel = new Vector2Int(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y));
            
            if (pixelPath.Count == 0 || pixelPath[pixelPath.Count - 1] != pixel)
            {
                pixelPath.Add(pixel);
            }
        }

        float radius = (currentTool == MapTool.Eraser) ? eraserRadius : penRadius;
        Color color = (currentTool == MapTool.Eraser) ? canvasBackgroundColor : activePenColor;
        Color? outline = (currentTool == MapTool.Eraser) ? null : (Color?)activeOutlineColor;

        // Pass 1: Outline
        if (outline.HasValue && outlineThickness > 0f)
        {
            float outerRadius = radius + outlineThickness;
            foreach (Vector2Int pt in pixelPath)
            {
                DrawAACircleOutline(pt.x, pt.y, outerRadius, outline.Value, color);
            }
        }

        // Pass 2: Fill Core
        foreach (Vector2Int pt in pixelPath)
        {
            DrawAACircleMain(pt.x, pt.y, radius, color);
        }
    }

    private void DrawAACircleOutline(int cx, int cy, float radius, Color outlineColor, Color mainPenColor)
    {
        int maxR = Mathf.CeilToInt(radius + 1f);

        for (int x = -maxR; x <= maxR; x++)
        {
            for (int y = -maxR; y <= maxR; y++)
            {
                float dist = Mathf.Sqrt(x * x + y * y);
                if (dist > radius + 0.5f) continue;

                int px = cx + x;
                int py = cy + y;

                if (px >= 0 && px < textureWidth && py >= 0 && py < textureHeight)
                {
                    Color bg = mapTexture.GetPixel(px, py);
                    if (bg == mainPenColor) continue;

                    float alpha = Mathf.Clamp01(radius + 0.5f - dist);
                    Color blended = Color.Lerp(bg, outlineColor, alpha * outlineColor.a);
                    mapTexture.SetPixel(px, py, blended);
                }
            }
        }
    }

    private void DrawAACircleMain(int cx, int cy, float radius, Color mainColor)
    {
        int maxR = Mathf.CeilToInt(radius + 1f);

        for (int x = -maxR; x <= maxR; x++)
        {
            for (int y = -maxR; y <= maxR; y++)
            {
                float dist = Mathf.Sqrt(x * x + y * y);
                if (dist > radius + 0.5f) continue;

                int px = cx + x;
                int py = cy + y;

                if (px >= 0 && px < textureWidth && py >= 0 && py < textureHeight)
                {
                    Color bg = mapTexture.GetPixel(px, py);
                    float alpha = Mathf.Clamp01(radius + 0.5f - dist);
                    Color blended = Color.Lerp(bg, mainColor, alpha * mainColor.a);
                    mapTexture.SetPixel(px, py, blended);
                }
            }
        }
    }

    private bool TryGetTexturePixel(Vector2 screenPos, Camera cam, out Vector2 pixelPos)
    {
        pixelPos = Vector2.zero;
        if (mapRectTransform == null) return false;

        bool isInside = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mapRectTransform, screenPos, cam, out Vector2 localPoint);

        if (!isInside) return false;

        Vector2 rectSize = mapRectTransform.rect.size;
        float u = (localPoint.x + rectSize.x * 0.5f) / rectSize.x;
        float v = (localPoint.y + rectSize.y * 0.5f) / rectSize.y;

        if (u < 0f || u > 1f || v < 0f || v > 1f) return false;

        pixelPos = new Vector2(u * textureWidth, v * textureHeight);
        return true;
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