using UnityEngine;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.DnnModule;
using OpenCVForUnity.UnityUtils;
using System.Collections.Generic;
using System.IO;
using OpenCVForUnity.ImgprocModule;
using UnityEngine.UI;

public class PersonDetection : MonoBehaviour
{
    public RawImage rawImageDisplay;
    public WebCamTexture webcamTexture;
    public string cfgFileName = "yolov4-tiny.cfg";
    public string weightsFileName = "yolov4-tiny.weights";
    public string namesFileName = "coco.names";

    private Net net;
    private string[] classNames;
    private Mat frame;
    private Size inputSize = new Size(416, 416);
    private float confThreshold = 0.5f;
    private bool personDetected = false;

    void Start()
    {
        webcamTexture = new WebCamTexture();
        webcamTexture.Play();
        rawImageDisplay.texture = webcamTexture;

        // สร้าง path ของไฟล์จาก StreamingAssets
        string cfgPath = Path.Combine(Application.streamingAssetsPath, cfgFileName);
        string weightsPath = Path.Combine(Application.streamingAssetsPath, weightsFileName);
        string namesPath = Path.Combine(Application.streamingAssetsPath, namesFileName);

        // โหลด YOLO model
        net = Dnn.readNetFromDarknet(cfgPath, weightsPath);

        // โหลดชื่อ class
        classNames = File.ReadAllLines(namesPath);
    }

    void Update()
    {
        if (webcamTexture.didUpdateThisFrame)
        {
            if (frame == null || frame.width() != webcamTexture.width || frame.height() != webcamTexture.height)
            {
                frame = new Mat(webcamTexture.height, webcamTexture.width, CvType.CV_8UC3);
            }
            
            Utils.webCamTextureToMat(webcamTexture, frame);
            DetectPeople(frame);
        }
    }

    void DetectPeople(Mat image)
    {
        personDetected = false;
        Imgproc.cvtColor(image, image, Imgproc.COLOR_RGBA2RGB);

        Mat blob = Dnn.blobFromImage(image, 1 / 255.0, inputSize, new Scalar(0, 0, 0), true, false);
        net.setInput(blob);

        List<Mat> outputs = new List<Mat>();
        net.forward(outputs, GetOutputNames(net));

        for (int i = 0; i < outputs.Count; ++i)
        {
            float[] data = new float[outputs[i].cols()];
            for (int j = 0; j < outputs[i].rows(); ++j)
            {
                outputs[i].get(j, 0, data);
                float confidence = data[4];
                if (confidence > confThreshold)
                {
                    int classId = ArgMax(data, 5);
                    string label = classNames[classId];
                    if (label == "person")
                    {
                        personDetected = true;
                        Debug.Log("🧍 Detected Person!");
                    }
                }
            }
        }
    }

    List<string> GetOutputNames(Net net)
    {
        List<string> names = new List<string>();
        MatOfInt outLayers = net.getUnconnectedOutLayers();
        List<string> layerNames = net.getLayerNames();

        for (int i = 0; i < outLayers.total(); i++)
        {
            int index = (int)outLayers.get(i, 0)[0];
            names.Add(layerNames[index - 1]);
        }

        return names;
    }
    
    int ArgMax(float[] data, int start)
    {
        int maxIndex = start;
        float maxScore = data[start];
        for (int i = start + 1; i < data.Length; i++)
        {
            if (data[i] > maxScore)
            {
                maxScore = data[i];
                maxIndex = i;
            }
        }
        return maxIndex;
    }
}