using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PuaseUIHandler : iGameStateListener
{
    GameObject pauseScreen;

    bool isPaused = false;
    bool isGameOver = false;
    bool isGameStarted = false;

    protected override void Start()
    {
        base.Start();

        pauseScreen = transform.GetChild(0).gameObject;
        pauseScreen.SetActive(false);

    }
    private void Update()
    {
        if (isGameOver)
            return;

        pauseScreen.SetActive(isPaused);
    }


    public override void HandleGameOver()
    {
        isGameOver = true;
    }

    public override void HandleGamePaused()
    {
        isPaused = true;
    }

    public override void HandleGameUnpaused()
    {
        isPaused=false;
    }

    public override void HangleGameStarted()
    {
        isGameStarted = true;

    }
}
