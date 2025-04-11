using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using Newtonsoft.Json;
using System.Threading;

public class UDPReceiver : MonoBehaviour
{
    public int port = 5055;
    private UdpClient udpClient;
    private Thread receiveThread;

    [Header("Perspective Reference")]
    public PerspectiveTransform perspectiveTransform;

    [Header("Prefab")]
    public GameObject personPrefab;

    private Dictionary<int, GameObject> personObjects = new Dictionary<int, GameObject>();

    void Start()
    {
        udpClient = new UdpClient(port);
        receiveThread = new Thread(new ThreadStart(ReceiveLoop));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    void ReceiveLoop()
    {
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, port);
        while (true)
        {
            try
            {
                byte[] data = udpClient.Receive(ref remoteEP);
                string json = Encoding.UTF8.GetString(data);
                Debug.Log("Received JSON: " + json);

                PersonData person = JsonConvert.DeserializeObject<PersonData>(json);
                HandlePersonData(person);
            }
            catch (Exception e)
            {
                Debug.LogError("UDP Receive Error: " + e.Message);
            }
        }
    }

    void HandlePersonData(PersonData person)
    {
        // 1. Perspective Transform (pixel to result space)
        Point inputPt = new Point(person.x, person.y);
        MatOfPoint2f src = new MatOfPoint2f(inputPt);
        MatOfPoint2f dst = new MatOfPoint2f();

        Core.perspectiveTransform(src, dst, perspectiveTransform.GetMatrix());
        Point worldPt = dst.toArray()[0];

        // 2. Call main thread for Unity object update
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            GameObject obj;
            if (!personObjects.TryGetValue(person.id, out obj))
            {
                obj = Instantiate(personPrefab, Vector3.zero, Quaternion.identity);
                personObjects.Add(person.id, obj);
            }

            // 3. Update position
            obj.transform.localPosition = new Vector3((float)worldPt.x, 0f, (float)worldPt.y);

            // 4. Update texture
            if (!string.IsNullOrEmpty(person.image))
            {
                byte[] imgData = Convert.FromBase64String(person.image);
                Texture2D tex = new Texture2D(2, 2);
                if (tex.LoadImage(imgData))
                {
                    Renderer rend = obj.GetComponent<Renderer>();
                    if (rend != null)
                        rend.material.mainTexture = tex;
                }
            }
        });
    }

    void OnApplicationQuit()
    {
        receiveThread.Abort();
        udpClient.Close();
    }

    [Serializable]
    public class PersonData
    {
        public int id;
        public int x;
        public int y;
        public string image;
    }
}
