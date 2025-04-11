using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI playerStatus;  // to show "Dead" or "Alive"
    public GameObject boxContainer;
    public List<Box> boxes = new List<Box>();
    public static GameManager Instance { get; private set; }

    private Coroutine gameLoopRoutine;

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

    // Call this method to start the game cycle.
    public void StartGame()
    {
        // Refill the boxes list.
        boxes.Clear();
        foreach (Transform child in boxContainer.transform)
        {
            Box boxComp = child.GetComponent<Box>();
            if (boxComp != null)
            {
                boxes.Add(boxComp);
            }
        }

        // Randomize each box to be safe or unsafe.
        foreach (Box box in boxes)
        {
            bool safe = (Random.Range(0, 2) == 0);
            box.isSafe = safe;
            box.ShowState();
        }

        playerStatus.text = "Box states are shown!";
        // Start the game loop.
        gameLoopRoutine = StartCoroutine(GameLoop());
    }

    // The Game Loop controls the timing of phases.
    IEnumerator GameLoop()
    {
        // Phase 1: Show box states for 5 seconds.
        yield return new WaitForSeconds(5f);

        // Phase 2: Hide unsafe boxes (simulate falling) for 5 seconds.
        foreach (Box box in boxes)
        {
            box.HideUnsafeBox();
        }
        playerStatus.text = "Unsafe boxes hidden! Step carefully!";
        yield return new WaitForSeconds(5f);

        // During or after this phase, external code (or an input event)
        // should call PlayPositionCheck(row, col) to check the player's step.
        // (If the player steps on a hidden unsafe box, they are marked as dead.)

        // Phase 3: Reset all boxes.
        foreach (Box box in boxes)
        {
            box.ResetBox();
        }
        playerStatus.text = "Boxes reset. You are safe!";
    }

    // Call this method to stop the game loop.
    public void StopGame()
    {
        if (gameLoopRoutine != null)
        {
            StopCoroutine(gameLoopRoutine);
            gameLoopRoutine = null;
        }
        playerStatus.text = "Game Stopped.";
    }

    // Call this method to immediately reset the game.
    public void ResetGame()
    {
        StopGame();
        foreach (Box box in boxes)
        {
            box.ResetBox();
        }
        playerStatus.text = "Game Reset.";
    }

    // Call this method when the player steps on a box (for example, using row & col)
    // It checks if the player stepped on an unsafe (and hidden) box.
    public void PlayPositionCheck(int row, int col)
    {
        foreach (Box box in boxes)
        {
            if (row == box.row && col == box.col)
            {
                // If the box is unsafe and its main object is hidden (unsafe box has fallen)
                if (!box.isSafe && !box.main.activeSelf)
                {
                    playerStatus.text = "Dead";  // Player falls and dies.
                    // You can add additional logic here to respawn the player.
                }
                else
                {
                    playerStatus.text = "Alive";
                }
            }
        }
    }
}
