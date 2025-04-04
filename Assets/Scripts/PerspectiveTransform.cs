using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity.DnnModule;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using TMPro;

public class PerspectiveTransform : MonoBehaviour
{
    public RawImage rawImageDisplay;
    public RawImage rawImageResult;
    private Mat sourceMat;

    private Vector2[] clickedPoints = new Vector2[4];
    public List<Image> markerImages;
    public TextMeshProUGUI markerPositionText;
    private int clickCount = 0;

    [Header("Grid Settings")] public int gridRows = 3;
    public int gridCols = 3;
    public Scalar gridColor = new Scalar(0, 255, 0, 255); // สีเขียว
    public int gridThickness = 2;

    [Header("Result Size")] public int resultWidthSize = 300;
    public int resultHeightSize = 300;

    [Header("Ui Element")] public TMP_InputField heightInputField;
    public TMP_InputField widthInputField;
    public TextMeshProUGUI trackingModeText;
    public TextMeshProUGUI pointingModeText;
    public Button confirmButton;

    private bool _trackingMode = false;
    private bool _pointingMode = false;
    private bool _isSetArea = false;
    private WebCamTexture webcamTexture;
    private Mat perspectiveMatrix;
    public RectTransform markerRectTransform;
    public RectTransform personMarker;

    Net yoloNet = YoloManager.Instance.yoloNet;
    List<string> classNames = YoloManager.Instance.classNames;

    void Start()
    {
        webcamTexture = new WebCamTexture();
        webcamTexture.Play();
        rawImageDisplay.texture = webcamTexture;

        confirmButton.onClick.AddListener(OnConfirmCustomResultSize);
        yoloNet = YoloManager.Instance.yoloNet;
        classNames = YoloManager.Instance.classNames;
    }

    void Update()
    {
        HandlePointClick();
        HandleGridClick();

        trackingModeText.text = "Tracking Mode: " + (_trackingMode ? "ON" : "OFF");
        pointingModeText.text = "Pointing Mode: " + (_pointingMode ? "ON" : "OFF");

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (_isSetArea)
            {
                _trackingMode = !_trackingMode;
                _pointingMode = false;
            }
            else
            {
                Debug.LogError($"Need to set area first");
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            _pointingMode = !_pointingMode;
            _trackingMode = false;
        }

        if (_trackingMode)
        {
            DetectWithYOLO();
            Vector2 mousePos = Input.mousePosition;
            RectTransform rect = rawImageDisplay.rectTransform;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, mousePos, null, out Vector2 localPos))
            {
                float x = (localPos.x + rect.rect.width / 2) * webcamTexture.width / rect.rect.width;
                float y = (localPos.y + rect.rect.height / 2) * webcamTexture.height / rect.rect.height;

                y = webcamTexture.height - y;

                MatOfPoint2f srcPoint = new MatOfPoint2f(new Point(x, y));
                MatOfPoint2f dstPoint = new MatOfPoint2f();

                Core.perspectiveTransform(srcPoint, dstPoint, perspectiveMatrix);

                Point resultPos = dstPoint.toArray()[0];

                float cellWidth = (float)resultWidthSize / gridCols;
                float cellHeight = (float)resultHeightSize / gridRows;

                int col = Mathf.FloorToInt((float)resultPos.x / cellWidth);
                int row = Mathf.FloorToInt((float)resultPos.y / cellHeight);

                col = Mathf.Clamp(col, 0, gridCols - 1);
                row = Mathf.Clamp(row, 0, gridRows - 1);

                markerPositionText.text = $"[{row},{col}]";

                RectTransform resultRect = rawImageResult.rectTransform;
                float texWidth = resultWidthSize;
                float texHeight = resultHeightSize;
                Vector2 pivotOffset = resultRect.pivot * resultRect.rect.size;

                float uiX = (float)(resultPos.x / texWidth) * resultRect.rect.width - pivotOffset.x;
                float uiY = (float)(1.0 - (resultPos.y / texHeight)) * resultRect.rect.height - pivotOffset.y;

                markerRectTransform.anchoredPosition = new Vector2(uiX, uiY);
            }
        }
    }

    void HandlePointClick()
    {
        if (_pointingMode)
        {
            if (Input.GetMouseButtonDown(0) && clickCount < 4)
            {
                Vector2 mousePos = Input.mousePosition;
                RectTransform rect = rawImageDisplay.rectTransform;

                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, mousePos, null, out Vector2 localPos))
                {
                    RectTransform markerRect = markerImages[clickCount].rectTransform;
                    markerRect.position = mousePos;

                    float x = (localPos.x + rect.rect.width / 2) * webcamTexture.width / rect.rect.width;
                    float y = (localPos.y + rect.rect.height / 2) * webcamTexture.height / rect.rect.height;

                    clickedPoints[clickCount] = new Vector2(x, webcamTexture.height - y);
                    clickCount++;

                    Debug.Log($"Point {clickCount}: ({x}, {webcamTexture.height - y})");

                    if (clickCount == 4)
                    {
                        ApplyPerspectiveTransform();
                        clickCount = 0;
                    }
                }
            }
        }
    }

    void HandleGridClick()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Vector2 mousePos = Input.mousePosition;
            RectTransform rect = rawImageResult.rectTransform;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, mousePos, null, out Vector2 localPos))
            {
                float texWidth = resultWidthSize;
                float texHeight = resultHeightSize;

                float x = (localPos.x + rect.rect.width / 2) * texWidth / rect.rect.width;
                float y = (localPos.y + rect.rect.height / 2) * texHeight / rect.rect.height;

                int col = Mathf.FloorToInt(x / (texWidth / gridCols));
                int row = Mathf.FloorToInt((texHeight - y) / (texHeight / gridRows));

                col = Mathf.Clamp(col, 0, gridCols - 1);
                row = Mathf.Clamp(row, 0, gridRows - 1);

                Debug.Log($"🟦 Clicked on Grid Cell: [Row {row}, Col {col}]");
            }
        }
    }

    public void OnConfirmCustomResultSize()
    {
        resultWidthSize = int.Parse(widthInputField.text);
        resultHeightSize = int.Parse(heightInputField.text);
        ApplyPerspectiveTransform();
    }

    void ApplyPerspectiveTransform()
    {
        Vector2[] sortedPoints = SortPointsClockwise(clickedPoints);

        Point[] srcPoints = new Point[4];
        for (int i = 0; i < 4; i++)
            srcPoints[i] = new Point(sortedPoints[i].x, sortedPoints[i].y);

        Point[] dstPoints = new Point[]
        {
            new Point(0, 0),
            new Point(resultWidthSize, 0),
            new Point(resultWidthSize, resultHeightSize),
            new Point(0, resultHeightSize)
        };

        MatOfPoint2f srcMat = new MatOfPoint2f(srcPoints);
        MatOfPoint2f dstMat = new MatOfPoint2f(dstPoints);

        perspectiveMatrix = Imgproc.getPerspectiveTransform(srcMat, dstMat);
        Mat output = new Mat(resultHeightSize, resultWidthSize, CvType.CV_8UC3);

        Mat cameraMat = new Mat(webcamTexture.height, webcamTexture.width, CvType.CV_8UC3);
        Utils.webCamTextureToMat(webcamTexture, cameraMat);

        Imgproc.warpPerspective(cameraMat, output, perspectiveMatrix, new Size(resultWidthSize, resultHeightSize));

        DrawGrid(output, gridRows, gridCols, gridColor, gridThickness);

        Texture2D resultTexture = new Texture2D(resultWidthSize, resultHeightSize, TextureFormat.RGB24, false);
        Utils.matToTexture2D(output, resultTexture);
        rawImageResult.texture = resultTexture;

        // Resize UI RawImage to match result size
        rawImageResult.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, resultWidthSize);
        rawImageResult.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, resultHeightSize);

        _isSetArea = true;
        _pointingMode = false;
    }

    Vector2[] SortPointsClockwise(Vector2[] points)
    {
        Vector2 center = Vector2.zero;
        foreach (var p in points)
            center += p;
        center /= points.Length;

        System.Array.Sort(points, (a, b) =>
        {
            float angleA = Mathf.Atan2(a.y - center.y, a.x - center.x);
            float angleB = Mathf.Atan2(b.y - center.y, b.x - center.x);
            return angleA.CompareTo(angleB);
        });

        return points;
    }

    void DrawGrid(Mat mat, int rows, int cols, Scalar color, int thickness)
    {
        int width = mat.width();
        int height = mat.height();

        float cellWidth = (float)width / cols;
        float cellHeight = (float)height / rows;

        for (int i = 1; i < cols; i++)
        {
            int x = Mathf.RoundToInt(cellWidth * i);
            Imgproc.line(mat, new Point(x, 0), new Point(x, height), color, thickness);
        }

        for (int j = 1; j < rows; j++)
        {
            int y = Mathf.RoundToInt(cellHeight * j);
            Imgproc.line(mat, new Point(0, y), new Point(width, y), color, thickness);
        }
    }

    void DetectWithYOLO()
    {
        if (yoloNet == null || perspectiveMatrix == null || !_isSetArea) return;

        Mat frame = new Mat(webcamTexture.height, webcamTexture.width, CvType.CV_8UC3);
        Utils.webCamTextureToMat(webcamTexture, frame);

        Size inputSize = new Size(416, 416);
        Scalar mean = new Scalar(0, 0, 0);
        float scale = 1 / 255.0f;

        Mat blob = Dnn.blobFromImage(frame, scale, inputSize, mean, true, false);
        yoloNet.setInput(blob);

        List<Mat> outputs = new List<Mat>();
        yoloNet.forward(outputs, yoloNet.getUnconnectedOutLayersNames());

        float confThreshold = 0.5f;
        float nmsThreshold = 0.4f;

        List<OpenCVForUnity.CoreModule.Rect> boxes = new List<OpenCVForUnity.CoreModule.Rect>();
        List<float> confidences = new List<float>();
        List<int> classIds = new List<int>();

        foreach (Mat output in outputs)
        {
            for (int i = 0; i < output.rows(); i++)
            {
                float[] data = new float[output.cols()];
                output.get(i, 0, data);

                float confidence = data[4];
                if (confidence > confThreshold)
                {
                    int classId = -1;
                    float maxClass = -1;
                    for (int j = 5; j < data.Length; j++)
                    {
                        if (data[j] > maxClass)
                        {
                            maxClass = data[j];
                            classId = j - 5;
                        }
                    }

                    if (classNames[classId] == "person")
                    {
                        float centerX = data[0] * frame.cols();
                        float centerY = data[1] * frame.rows();
                        float width = data[2] * frame.cols();
                        float height = data[3] * frame.rows();

                        int x = Mathf.FloorToInt(centerX - width / 2);
                        int y = Mathf.FloorToInt(centerY - height / 2);

                        boxes.Add(new OpenCVForUnity.CoreModule.Rect(x, y, (int)width, (int)height));
                        confidences.Add(confidence);
                        classIds.Add(classId);
                    }
                }
            }
        }

        MatOfRect2d rect2d = new MatOfRect2d();
        foreach (var r in boxes)
            rect2d.push_back(new MatOfRect2d(new Rect2d(r.x, r.y, r.width, r.height)));

        MatOfFloat confs = new MatOfFloat(confidences.ToArray());
        MatOfInt indices = new MatOfInt();

        Dnn.NMSBoxes(rect2d, confs, confThreshold, nmsThreshold, indices);

        foreach (int idx in indices.toArray())
        {
            OpenCVForUnity.CoreModule.Rect box = boxes[idx];
            Point center = new Point(box.x + box.width / 2, box.y + box.height / 2);
            
            // แปลงผ่าน perspective matrix
            MatOfPoint2f src = new MatOfPoint2f(center);
            MatOfPoint2f dst = new MatOfPoint2f();
            Core.perspectiveTransform(src, dst, perspectiveMatrix);
            Point resultPos = dst.toArray()[0];
            
            RectTransform resultRect = rawImageResult.rectTransform;
            Vector2 pivotOffset = resultRect.pivot * resultRect.rect.size;
            
            float uiX = (float)(resultPos.x / resultWidthSize) * resultRect.rect.width - pivotOffset.x;
            float uiY = (float)(1.0 - (resultPos.y / resultHeightSize)) * resultRect.rect.height - pivotOffset.y;

            personMarker.anchoredPosition = new Vector2(uiX, uiY);

            // คำนวณ grid
            float cellWidth = (float)resultWidthSize / gridCols;
            float cellHeight = (float)resultHeightSize / gridRows;

            int col = Mathf.FloorToInt((float)resultPos.x / cellWidth);
            int row = Mathf.FloorToInt((float)resultPos.y / cellHeight);

            col = Mathf.Clamp(col, 0, gridCols - 1);
            row = Mathf.Clamp(row, 0, gridRows - 1);

            Debug.Log($"🧍 YOLO person detected in Grid Cell: [Row {row}, Col {col}]");
        }
    }
}
