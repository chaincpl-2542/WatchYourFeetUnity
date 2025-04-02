using UnityEngine;
using OpenCVForUnity.CoreModule;
using TMPro;

public class PersonPositionTransformer : MonoBehaviour
{
    [Header("Perspective Settings")]
    public PerspectiveTransform perspectiveTransform; // drag ตัว PerspectiveTransform.cs มาใส่

    [Header("Target Marker UI")]
    public RectTransform gridMarkerUI; // marker UI ที่จะไปแสดงตำแหน่งบน grid

    [Header("Grid Display")]
    public TextMeshProUGUI debugGridText;

    [Header("Grid Size")] 
    public int gridRows = 3;
    public int gridCols = 3;

    [Header("Texture Size")] 
    public int resultWidthSize = 300;
    public int resultHeightSize = 300;

    public void UpdatePosition(Vector2 screenPosition)
    {
        if (perspectiveTransform == null || perspectiveTransform.GetMatrix() == null)
        {
            Debug.LogWarning("PerspectiveTransform matrix not set.");
            return;
        }

        // 1. สร้างจุดจาก screen position ที่ได้ (จาก MyPersonDetection)
        MatOfPoint2f srcPoint = new MatOfPoint2f(new Point(screenPosition.x, screenPosition.y));
        MatOfPoint2f dstPoint = new MatOfPoint2f();

        // 2. ใช้ perspective matrix
        Core.perspectiveTransform(srcPoint, dstPoint, perspectiveTransform.GetMatrix());
        Point resultPos = dstPoint.toArray()[0];

        // 3. คำนวณตำแหน่งบน Grid
        float cellWidth = (float)resultWidthSize / gridCols;
        float cellHeight = (float)resultHeightSize / gridRows;

        int col = Mathf.FloorToInt((float)resultPos.x / cellWidth);
        int row = Mathf.FloorToInt((float)resultPos.y / cellHeight);

        col = Mathf.Clamp(col, 0, gridCols - 1);
        row = Mathf.Clamp(row, 0, gridRows - 1);

        if (debugGridText != null)
            debugGridText.text = $"[Row: {row}, Col: {col}]";

        // 4. แสดง marker UI (optional)
        if (gridMarkerUI != null)
        {
            RectTransform resultRect = perspectiveTransform.rawImageResult.rectTransform;
            float texWidth = resultWidthSize;
            float texHeight = resultHeightSize;
            Vector2 pivotOffset = resultRect.pivot * resultRect.rect.size;

            float uiX = (float)(resultPos.x / texWidth) * resultRect.rect.width - pivotOffset.x;
            float uiY = (float)(1.0 - (resultPos.y / texHeight)) * resultRect.rect.height - pivotOffset.y;

            gridMarkerUI.anchoredPosition = new Vector2(uiX, uiY);
        }
    }
}