using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    public float sensX = 1.0f;
    public float sensY = 1.0f;

    public Transform orientation;

    [Header("Cache")]
    [SerializeField, ReadOnly]float xRot;
    [SerializeField, ReadOnly]float yRot;

    [ReadOnly] public float Tilt = 0;

    PlayerInputDispatcher dispatcher;

    private void Start()
    {
        dispatcher = FindObjectOfType<PlayerInputDispatcher>();
        
        dispatcher.OnLookInputRecieved += OnLookInputRecieved;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }

    void OnLookInputRecieved(Vector2 newLookDir)
    {
        float mosueX = newLookDir.x * Time.deltaTime * sensX;
        float mouseY = newLookDir.y * Time.deltaTime * sensY;

        yRot += mosueX;
        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -90, 60);
    }


    private void Update()
    {
        transform.localRotation = Quaternion.Euler(xRot, yRot, Tilt);
        orientation.rotation = Quaternion.Euler(0, yRot, 0);
    }



}
