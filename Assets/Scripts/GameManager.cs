using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI playerStatus;
    public GameObject boxContainer;
    public List<Box> boxes = new List<Box>();
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void StartGame()
    {
        foreach (Transform child in boxContainer.transform)
        {
            boxes.Add(child.GetComponent<Box>());
        }
        RandomBoxState();
    }

    public void PlayPositionCheck(int row, int col)
    {
        for (int i = 0; i < boxes.Count; i++)
        {
            if (row == boxes[i].row && col == boxes[i].col)
            {
                playerStatus.text = $"IsDead : {boxes[i].isSafe.ToString()}";
            }
        }
    }

    public void RandomBoxState()
    {
        for (int i = 0; i < boxes.Count; i++)
        {
            boxes[i].main.SetActive(true);
            var value = Random.Range(0, 2);
            if (value == 0)
            {
                boxes[i].SetBoxState(true);
            }
            else
            {
                boxes[i].SetBoxState(false);
            }
        }
    }
}
