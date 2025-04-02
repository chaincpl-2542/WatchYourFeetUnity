using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UnityUtils.Helper;
using OpenCVForUnityExample.DnnModel;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using KeyPoint = OpenCVForUnityExample.DnnModel.MediaPipePoseEstimator.KeyPoint;

namespace OpenCVForUnityExample
{
    [RequireComponent(typeof(MultiSource2MatHelper))]
    public class MyPersonDetectionWithRawImage : MonoBehaviour
    {
        public RawImage rawImageDisplay;
        public Image markerPrefab;

        private Image leftFootMarker;
        private Image rightFootMarker;

        public bool mask;
        public bool showSkeleton;
        public MediaPipePoseSkeletonVisualizer skeletonVisualizer;
        public ARHelper arHelper;

        private MultiSource2MatHelper multiSource2MatHelper;
        private Texture2D texture;
        private Mat bgrMat;

        private MediaPipePersonDetector personDetector;
        private MediaPipePoseEstimator poseEstimator;

        private static readonly string PERSON_DETECTION_MODEL_FILENAME = "OpenCVForUnity/dnn/person_detection_mediapipe_2023mar.onnx";
        private static readonly string POSE_ESTIMATION_MODEL_FILENAME = "OpenCVForUnity/dnn/pose_estimation_mediapipe_2023mar.onnx";

        private string person_detection_model_filepath;
        private string pose_estimation_model_filepath;

        private CancellationTokenSource cts = new CancellationTokenSource();

        async void Start()
        {
            multiSource2MatHelper = GetComponent<MultiSource2MatHelper>();
            multiSource2MatHelper.outputColorFormat = Source2MatHelperColorFormat.RGBA;

            if (skeletonVisualizer != null) skeletonVisualizer.showSkeleton = showSkeleton;

            person_detection_model_filepath = await Utils.getFilePathAsyncTask(PERSON_DETECTION_MODEL_FILENAME, cancellationToken: cts.Token);
            pose_estimation_model_filepath = await Utils.getFilePathAsyncTask(POSE_ESTIMATION_MODEL_FILENAME, cancellationToken: cts.Token);

            Run();

            leftFootMarker = Instantiate(markerPrefab, markerPrefab.transform.parent);
            rightFootMarker = Instantiate(markerPrefab, markerPrefab.transform.parent);
            leftFootMarker.gameObject.SetActive(true);
            rightFootMarker.gameObject.SetActive(true);
        }

        void Run()
        {
            Utils.setDebugMode(true);

            if (!string.IsNullOrEmpty(person_detection_model_filepath))
                personDetector = new MediaPipePersonDetector(person_detection_model_filepath, 0.3f, 0.6f, 5000);

            if (!string.IsNullOrEmpty(pose_estimation_model_filepath))
                poseEstimator = new MediaPipePoseEstimator(pose_estimation_model_filepath, 0.9f);

            multiSource2MatHelper.Initialize();
        }

        public void OnSourceToMatHelperInitialized()
        {
            Mat rgbaMat = multiSource2MatHelper.GetMat();

            texture = new Texture2D(rgbaMat.cols(), rgbaMat.rows(), TextureFormat.RGBA32, false);
            Utils.matToTexture2D(rgbaMat, texture);

            rawImageDisplay.texture = texture;
            rawImageDisplay.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rgbaMat.cols());
            rawImageDisplay.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rgbaMat.rows());

            bgrMat = new Mat(rgbaMat.rows(), rgbaMat.cols(), CvType.CV_8UC3);
            arHelper.Initialize(Screen.width, Screen.height, rgbaMat.width(), rgbaMat.height(), new double[0]);
        }

        void Update()
        {
            if (multiSource2MatHelper.IsPlaying() && multiSource2MatHelper.DidUpdateThisFrame())
            {
                Mat rgbaMat = multiSource2MatHelper.GetMat();

                if (personDetector == null || poseEstimator == null)
                {
                    Imgproc.putText(rgbaMat, "model file is not loaded.", new Point(5, rgbaMat.rows() - 30), Imgproc.FONT_HERSHEY_SIMPLEX, 0.7, new Scalar(255, 255, 255, 255), 2);
                    Imgproc.putText(rgbaMat, "Please read console message.", new Point(5, rgbaMat.rows() - 10), Imgproc.FONT_HERSHEY_SIMPLEX, 0.7, new Scalar(255, 255, 255, 255), 2);
                }
                else
                {
                    Imgproc.cvtColor(rgbaMat, bgrMat, Imgproc.COLOR_RGBA2BGR);
                    Mat persons = personDetector.infer(bgrMat);

                    List<Mat> poses = new List<Mat>();
                    for (int i = 0; i < persons.rows(); ++i)
                    {
                        List<Mat> results = poseEstimator.infer(bgrMat, persons.row(i), mask);
                        poses.Add(results[0]);
                    }

                    Imgproc.cvtColor(bgrMat, rgbaMat, Imgproc.COLOR_BGR2RGBA);

                    foreach (var pose in poses)
                        poseEstimator.visualize(rgbaMat, pose, false, true);

                    if (skeletonVisualizer != null && skeletonVisualizer.showSkeleton && poses.Count > 0 && !poses[0].empty())
                    {
                        var data = poseEstimator.getData(poses[0]);
                        var landmarks_screen = data.landmarks_screen;

                        Vector2 leftFoot = new Vector2(landmarks_screen[(int)KeyPoint.LeftFootIndex].x, landmarks_screen[(int)KeyPoint.LeftFootIndex].y);
                        Vector2 rightFoot = new Vector2(landmarks_screen[(int)KeyPoint.RightFootIndex].x, landmarks_screen[(int)KeyPoint.RightFootIndex].y);

                        RectTransform rect = rawImageDisplay.rectTransform;

                        Vector2 localLeft;
                        Vector2 localRight;

                        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, new Vector2(leftFoot.x, Screen.height - leftFoot.y), null, out localLeft);
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, new Vector2(rightFoot.x, Screen.height - rightFoot.y), null, out localRight);

                        leftFootMarker.rectTransform.anchoredPosition = localLeft;
                        rightFootMarker.rectTransform.anchoredPosition = localRight;
                    }
                }

                Utils.matToTexture2D(rgbaMat, texture);
            }
        }

        void OnDestroy()
        {
            multiSource2MatHelper.Dispose();
            personDetector?.dispose();
            poseEstimator?.dispose();
            Utils.setDebugMode(false);
            cts?.Dispose();
        }
    }
}