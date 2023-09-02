using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    public float sensX = 1.0f;
    public float sensY = 1.0f;

    public Transform orientation;

    float xRot;
    float yRot;
    float tilt = 0;

    PlayerWallRun wallRun;

    private void Start()
    {
        wallRun = FindObjectOfType<PlayerWallRun>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        float mosueX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRot += mosueX;
        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -90, 90);

        transform.localRotation = Quaternion.Euler(xRot, yRot, tilt);
        orientation.rotation = Quaternion.Euler(0, yRot, 0);

        HandleCameraTilt();
    }

    void HandleCameraTilt()
    {

        if (Mathf.Abs(wallRun.wallRunCameraTilt) < wallRun.maxWallRunCameraTilt && wallRun.isWallRunning && wallRun.isWallRight)
        {
            wallRun.wallRunCameraTilt += Time.deltaTime * wallRun.maxWallRunCameraTilt * 20f;
        }
        if (Mathf.Abs(wallRun.wallRunCameraTilt) < wallRun.maxWallRunCameraTilt && wallRun.isWallRunning && wallRun.isWallLeft)
        {
            wallRun.wallRunCameraTilt -= Time.deltaTime * wallRun.maxWallRunCameraTilt * 20f;
        }

        if (wallRun.wallRunCameraTilt > 0 && !wallRun.isWallRight && !wallRun.isWallLeft)
            wallRun.wallRunCameraTilt -= Time.deltaTime * wallRun.maxWallRunCameraTilt * 20f;
        if (wallRun.wallRunCameraTilt < 0 && !wallRun.isWallLeft && !wallRun.isWallRight)
            wallRun.wallRunCameraTilt += Time.deltaTime * wallRun.maxWallRunCameraTilt * 20f;
        tilt = wallRun.wallRunCameraTilt;
    }

}
