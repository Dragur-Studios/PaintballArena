using System;
using UnityEngine;

public class PlayerWallRun : MonoBehaviour
{
    [SerializeField] Transform orientation;
    [SerializeField] Rigidbody rigi;
    
    [SerializeField] LayerMask wallLayer;
    public float wallRunForce, maxWallRunTime, maxWallSpeed;
    public float maxWallRunCameraTilt, wallRunCameraTilt;
    public bool isWallRight, isWallLeft;
    public bool isWallRunning = false;

    private void Start()
    {
        rigi = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if(Input.GetKey(KeyCode.D) && isWallRight)
        {
            StartWallRun();
        }
        else if(Input.GetKey(KeyCode.A) && isWallLeft)
        {
            StartWallRun();
        }


        CheckForWalls();
    }




    void StartWallRun()
    {
        rigi.useGravity = false;
        isWallRunning = true;

        if(rigi.velocity.magnitude <= maxWallSpeed)
        {
            rigi.AddForce(orientation.forward * wallRunForce * Time.deltaTime);

            if (isWallLeft)
            {
                rigi.AddForce(-orientation.right * wallRunForce / 5 * Time.deltaTime);
            }
            else if (isWallRight)
            {
                rigi.AddForce(orientation.right * wallRunForce / 5 * Time.deltaTime);
            }
        }
    }

    void StopWallRunning()
    {
        rigi.useGravity = true;
        isWallRunning = false;

    }
    void CheckForWalls()
    {
        isWallRight = Physics.Raycast(transform.position, orientation.right, 1.0f, wallLayer);
        isWallLeft = Physics.Raycast(transform.position, -orientation.right, 1.0f, wallLayer);
        
        if(!isWallRight && !isWallLeft)
            StopWallRunning();
    }

}
