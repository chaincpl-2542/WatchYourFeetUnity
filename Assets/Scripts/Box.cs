using UnityEngine;

public class Box : MonoBehaviour
{
    public int row;
    public int col;
    public bool isSafe;

    public GameObject cross;    // indicator for unsafe
    public GameObject correct;  // indicator for safe
    public GameObject main;     // the visible box itself

    private void Start()
    {
        // Assume the box starts as safe.
        isSafe = true;
        main.SetActive(true);
        correct.SetActive(false);
        cross.SetActive(false);
    }

    // Called when displaying the current state (safe or unsafe)
    public void ShowState()
    {
        main.SetActive(true);
        if (isSafe)
        {
            correct.SetActive(true);
            cross.SetActive(false);
        }
        else
        {
            correct.SetActive(false);
            cross.SetActive(true);
        }
    }

    // Called to hide unsafe boxes (simulate falling)
    public void HideUnsafeBox()
    {
        if (!isSafe)
        {
            main.SetActive(false);
        }
        // Safe boxes remain visible.
    }

    // Called to reset the box (set to safe)
    public void ResetBox()
    {
        isSafe = true;
        main.SetActive(true);
        correct.SetActive(false);
        cross.SetActive(false);
    }
}