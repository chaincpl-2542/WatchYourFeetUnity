using System;
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
    
    private int resultWidthSize;
    private int resultHeightSize;

    public void UpdatePosition(Vector2 screenPosition)
    {
        if (perspectiveTransform == null || perspectiveTransform.GetMatrix() == null)
        {
            Debug.LogWarning("PerspectiveTransform matrix not set.");
            return;
        }

        resultWidthSize = perspectiveTransform.resultWidthSize;
        resultHeightSize = perspectiveTransform.resultHeightSize;
        
        var rawRect = perspectiveTransform.rawImageDisplayUI.rectTransform;
        var camMat = perspectiveTransform.GetCameraMat();
        if (camMat == null)
        {
            Debug.LogWarning("Camera Mat is null");
            return;
        }

        float camWidth = camMat.width();
        float camHeight = camMat.height();

        // Convert screenPosition to match rawImage rect
        float x = (screenPosition.x / rawRect.rect.width) * camWidth;
        float y = (screenPosition.y / rawRect.rect.height) * camHeight;

        print($"Person x : {x} y : {y} ");
        
        // Apply perspective transform
        MatOfPoint2f srcPoint = new MatOfPoint2f(new Point(x, y));
        MatOfPoint2f dstPoint = new MatOfPoint2f();
        Core.perspectiveTransform(srcPoint, dstPoint, perspectiveTransform.GetMatrix());
        Point resultPos = dstPoint.toArray()[0];

        // Compute grid cell
        float cellWidth = (float)resultWidthSize / gridCols;
        float cellHeight = (float)resultHeightSize / gridRows;

        int col = Mathf.FloorToInt((float)resultPos.x / cellWidth);
        int row = Mathf.FloorToInt((float)resultPos.y / cellHeight);
        
        col = Mathf.Clamp(col, 0, gridCols - 1);
        row = Mathf.Clamp(row, 0, gridRows - 1);

        if (debugGridText != null)
            debugGridText.text = $"[Row: {row}, Col: {col}]";

        if (gridMarkerUI != null)
        {
            RectTransform resultRect = perspectiveTransform.rawImageResult.rectTransform;
            float texWidth = resultWidthSize;
            float texHeight = resultHeightSize;
            Vector2 pivotOffset = resultRect.pivot * resultRect.rect.size;

            float uiX = (float)(resultPos.x / texWidth) * resultRect.rect.width - pivotOffset.x;
            float uiY = (float)(1.0 - (resultPos.y / texHeight)) * resultRect.rect.height - pivotOffset.y;

            gridMarkerUI.anchoredPosition = new Vector2(uiX, uiY);

            GameManager.Instance.PlayPositionCheck(row,col);
        }
    }
}
