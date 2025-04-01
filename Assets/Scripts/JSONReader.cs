using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class JSONReader : MonoBehaviour
{
    public GameObject personPrefab;
    private Dictionary<int, GameObject> trackedPeople = new Dictionary<int, GameObject>();

    void Update()
    {
        string path = Application.dataPath + "/../output.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            var people = JsonConvert.DeserializeObject<List<TrackedPerson>>(json);

            foreach (var p in people)
            {
                if (!trackedPeople.ContainsKey(p.id))
                {
                    GameObject newPerson = Instantiate(personPrefab);
                    trackedPeople[p.id] = newPerson;
                }

                Vector3 pos = new Vector3(p.x / 100.0f, 0, p.y / 100.0f);
                trackedPeople[p.id].transform.position = pos;
            }
        }
    }

    public class TrackedPerson
    {
        public int id;
        public int x;
        public int y;
        public int w;
        public int h;
    }
}