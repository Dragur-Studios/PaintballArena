using System;
using UnityEngine;

public abstract class iGameStateListener : MonoBehaviour
{
    protected virtual void Start()
    {
        GameManager.AddGameStateListener(this);
    }

    public abstract void HandleGameOver();

    public abstract void HangleGameStarted();
    public abstract void HandleGamePaused();
    public abstract void HandleGameUnpaused();

}
