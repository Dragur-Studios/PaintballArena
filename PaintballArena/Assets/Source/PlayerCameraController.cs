using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;

public class PlayerCameraController : iGameStateListener
{
    public float sensX = 1.0f;
    public float sensY = 1.0f;

    public Transform orientation;

    [Header("Cache")]
    [SerializeField, ReadOnly]float xRot;
    [SerializeField, ReadOnly]float yRot;

    [ReadOnly] public float Tilt = 0;

    PlayerInputDispatcher dispatcher;
    PlayerLocomotion locomotion;
    Player player;

    bool isGameOver = false;
    bool isGamePaused = false;
    bool isGameStarted = false;

    protected override void Start()
    {
        base.Start();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }

    public void OnLookInputRecieved(Vector2 newLookDir)
    {
        if (isGameOver || isGamePaused || !isGameStarted)
            return;

        float mosueX = newLookDir.x * Time.deltaTime * sensX;
        float mouseY = newLookDir.y * Time.deltaTime * sensY;

        yRot += mosueX;
        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -90, 90);
    }


    private void Update()
    {
        if (isGameOver || isGamePaused || !isGameStarted)
            return;


        transform.localRotation = Quaternion.Euler(xRot, yRot, Tilt);
        orientation.rotation = Quaternion.Euler(0, yRot, 0);
    }



    public override void HandleGameOver()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        isGameOver = true;
    }

    public override void HandleGamePaused()
    {
        isGamePaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public override void HandleGameUnpaused()
    {
        isGamePaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public override void HangleGameStarted()
    {
        isGameStarted = true;
    }
}
