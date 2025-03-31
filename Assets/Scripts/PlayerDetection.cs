using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.DnnModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;

public class PlayerDetection : MonoBehaviour
{
    public WebCamTexture webcamTexture;
    public Mat perspectiveMatrix;
    public List<string> classNames;
    public Net yoloNet;

    [Header("Grid Settings")]
    public int resultWidthSize = 300;
    public int resultHeightSize = 300;
    public int gridCols = 3;
    public int gridRows = 3;

    void Update()
    {
        if (webcamTexture != null && perspectiveMatrix != null && yoloNet != null)
        {
            DetectPlayerFromCamera();
        }
    }

    void DetectPlayerFromCamera()
    {
        Mat frame = new Mat(webcamTexture.height, webcamTexture.width, CvType.CV_8UC3);
        Utils.webCamTextureToMat(webcamTexture, frame);

        Size inpSize = new Size(416, 416);
        Scalar mean = new Scalar(0, 0, 0);
        float scale = 1 / 255.0f;

        Mat blob = Dnn.blobFromImage(frame, scale, inpSize, mean, true, false);
        yoloNet.setInput(blob);

        List<Mat> outs = new List<Mat>();
        yoloNet.forward(outs, yoloNet.getUnconnectedOutLayersNames());

        float confThreshold = 0.4f;
        float nmsThreshold = 0.4f;

        List<OpenCVForUnity.CoreModule.Rect> boxes = new List<OpenCVForUnity.CoreModule.Rect>();
        List<float> confidences = new List<float>();
        List<int> classIds = new List<int>();

        foreach (Mat outMat in outs)
        {
            for (int i = 0; i < outMat.rows(); i++)
            {
                float confidence = (float)outMat.get(i, 4)[0];
                if (confidence > confThreshold)
                {
                    float[] data = new float[outMat.cols()];
                    outMat.get(i, 0, data);

                    float maxVal = float.MinValue;
                    int maxClass = -1;
                    for (int j = 5; j < data.Length; j++)
                    {
                        if (data[j] > maxVal)
                        {
                            maxVal = data[j];
                            maxClass = j - 5;
                        }
                    }

                    if (classNames[maxClass] != "person") continue;

                    int centerX = (int)(data[0] * frame.cols());
                    int centerY = (int)(data[1] * frame.rows());
                    int width = (int)(data[2] * frame.cols());
                    int height = (int)(data[3] * frame.rows());

                    int x = centerX - width / 2;
                    int y = centerY - height / 2;

                    boxes.Add(new OpenCVForUnity.CoreModule.Rect(x, y, width, height));
                    confidences.Add(confidence);
                    classIds.Add(maxClass);
                }
            }
        }

        MatOfRect2d rect2d = new MatOfRect2d();
        foreach (var r in boxes)
            rect2d.push_back(new MatOfRect2d(new Rect2d(r.x, r.y, r.width, r.height)));

        MatOfFloat confs = new MatOfFloat(confidences.ToArray());
        MatOfInt indices = new MatOfInt();

        Dnn.NMSBoxes(rect2d, confs, confThreshold, nmsThreshold, indices);

        foreach (int i in indices.toArray())
        {
            OpenCVForUnity.CoreModule.Rect box = boxes[i];
            Point center = new Point(box.x + box.width / 2, box.y + box.height / 2);

            MatOfPoint2f src = new MatOfPoint2f(center);
            MatOfPoint2f dst = new MatOfPoint2f();
            Core.perspectiveTransform(src, dst, perspectiveMatrix);
            Point result = dst.toArray()[0];

            float cellWidth = (float)resultWidthSize / gridCols;
            float cellHeight = (float)resultHeightSize / gridRows;

            int col = Mathf.FloorToInt((float)result.x / cellWidth);
            int row = Mathf.FloorToInt((float)result.y / cellHeight);

            col = Mathf.Clamp(col, 0, gridCols - 1);
            row = Mathf.Clamp(row, 0, gridRows - 1);

            Debug.Log($"🧍 Person at: [Row {row}, Col {col}]");
        }
    }
}