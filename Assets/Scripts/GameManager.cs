using System.Collections;
using System.Collections.Generic;
using OpenCVForUnityExample;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI playerStatus;
    public GameObject boxContainer;
    public List<Box> boxes = new List<Box>();
    public static GameManager Instance { get; private set; }

    private Coroutine gameLoopRoutine;
    private bool checkDead;
    private bool _isGameStarted;
    private int _round;
    public int maxRound = 10;
    public float timerDecreasePerRound = 0.2f;
    
    public TextMeshProUGUI roundText;

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
        if (!_isGameStarted)
        {
            _isGameStarted = true;
            boxes.Clear();
            foreach (Transform child in boxContainer.transform)
            {
                Box boxComp = child.GetComponent<Box>();
                if (boxComp != null)
                {
                    boxes.Add(boxComp);
                }
            }

            foreach (Box box in boxes)
            {
                bool safe = (Random.Range(0, 2) == 0);
                box.isSafe = safe;
                box.ShowState();
            }

            gameLoopRoutine = StartCoroutine(GameLoop());
        }
    }

    IEnumerator GameLoop()
    {
        while (_round < maxRound)
        {
            _round += 1;
            Debug.Log("Round: " + _round);

            
            yield return new WaitForSeconds(5f - timerDecreasePerRound * (_round - 1));

            foreach (Box box in boxes)
            {
                box.HideUnsafeBox();
            }

            yield return new WaitForSeconds(5f- timerDecreasePerRound * (_round - 1));

            foreach (Box box in boxes)
            {
                box.ResetBox();
            }

            checkDead = false;
        }
        
        Debug.Log("Game Over");
        _isGameStarted = false;
        _round = 0;
    }

    public void StopGame()
    {
        _isGameStarted = false;
        _round = 0;
        checkDead = false;
        if (gameLoopRoutine != null)
        {
            StopCoroutine(gameLoopRoutine);
            gameLoopRoutine = null;
        }
        playerStatus.text = "Game Stopped.";
    }

    public void ResetGame()
    {
        StopGame();
        foreach (Box box in boxes)
        {
            box.ResetBox();
        }
        playerStatus.text = "Game Reset.";
    }

    public void PlayPositionCheck(int row, int col)
    {
        foreach (Box box in boxes)
        {
            if (row == box.row && col == box.col)
            {
                print(box.isSafe);
                if (!box.isSafe && !box.main.activeSelf)
                {
                    if (checkDead == false)
                    {
                        MyPersonDetection detection = FindObjectOfType<MyPersonDetection>();
                        if (detection != null)
                        {
                            detection.HandlePlayerDeath(box.gameObject.transform.position);
                        }

                        playerStatus.text = "Dead";
                        checkDead = true;
                    }
                }
                else
                {
                    playerStatus.text = "Alive";
                }
            }
        }
    }
}
