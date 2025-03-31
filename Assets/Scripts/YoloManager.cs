using UnityEngine;
using System.Collections.Generic;
using OpenCVForUnity.DnnModule;
using System.IO;
using System.Linq;
using OpenCVForUnity.UnityUtils;

public class YoloManager : MonoBehaviour
{
    public static YoloManager Instance;

    public  Net yoloNet;
    public  List<string> classNames;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadYOLO();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void LoadYOLO()
    {
        string cfgPath = Utils.getFilePath("yolo/yolov4-tiny.cfg");
        string weightsPath = Utils.getFilePath("yolo/yolov4-tiny.weights");
        string namesPath = Utils.getFilePath("yolo/coco.names");

        if (!File.Exists(cfgPath) || !File.Exists(weightsPath) || !File.Exists(namesPath))
        {
            Debug.LogError("❌ YOLO files not found in StreamingAssets/yolo/");
            return;
        }

        yoloNet = Dnn.readNetFromDarknet(cfgPath, weightsPath);
        yoloNet.setPreferableBackend(Dnn.DNN_BACKEND_OPENCV);
        yoloNet.setPreferableTarget(Dnn.DNN_TARGET_CPU);

        classNames = File.ReadAllLines(namesPath).ToList();

        Debug.Log("✅ YOLO Loaded Once and Ready!");
    }
}
