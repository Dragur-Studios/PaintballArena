using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using UnityEngine;

class GameManager : MonoBehaviour
{
    List<iGameStateListener> listeners = new List<iGameStateListener>();

    bool isPaused = false;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

    }


    public static void SignalGameOver()
    {
        instance.listeners.ForEach(l => l?.HandleGameOver());
    }

    public static void SignalGamePaused()
    {
        Time.timeScale = 0.0f;
        instance.listeners.ForEach(l => l?.HandleGamePaused());
    }


    internal static void AddGameStateListener(iGameStateListener listener)
    {
        if (instance.listeners.Contains(listener))
            return;

        instance.listeners.Add(listener);
    }

    public static void SignalGameUnaused()
    {
        Time.timeScale = 1.0f;
        instance.listeners.ForEach(l => l?.HandleGameUnpaused());
    }

    static GameManager instance;

}
