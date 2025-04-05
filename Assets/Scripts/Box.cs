using System;
using System.Collections;
using UnityEngine;

public class Box : MonoBehaviour
{
    public int row;
    public int col;
    public bool isSafe;

    public GameObject cross;
    public GameObject correct;
    public GameObject main;

    public float delayTime = 3f;
    public float delayResetTime = 3f;

    private bool check;

    private void Start()
    {
        correct.SetActive(false);
        cross.SetActive(false);
    }

    public void SetBoxState(bool state)
    {
        isSafe = state;
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

        if (!check)
        {
            StartCoroutine(Delay(state));
            check = true;
        }
    }

    public IEnumerator Delay(bool state)
    {
        yield return new WaitForSeconds(delayTime);
        main.SetActive(state);
        isSafe = state;
        
        yield return new WaitForSeconds(delayResetTime);
        main.SetActive(true);
        correct.SetActive(false);
        cross.SetActive(false);
        isSafe = true;
        check = false;
    }
}
