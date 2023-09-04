using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuaseUIHandler : MonoBehaviour
{
    PlayerInputDispatcher dispatcher;
    GameObject pauseScreen;

    private void Start()
    {
        pauseScreen = transform.GetChild(0).gameObject;
        pauseScreen.SetActive(false);

        dispatcher = FindObjectOfType<PlayerInputDispatcher>();
        dispatcher.OnPauseInputRecieved += OnPauseButtonRecieved;
    }

    bool isPaused = false;

    void OnPauseButtonRecieved(bool shouldPause)
    {
        if (shouldPause && !isPaused)
        {
            isPaused = true;
            pauseScreen.SetActive(isPaused);
            

            GameManager.SignalGamePaused();
        }
        else if (shouldPause && isPaused)
        {
            isPaused = false;
            pauseScreen.SetActive(isPaused);

            GameManager.SignalGameUnaused();
        }
    }
}
