using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using Unity.VisualScripting;
using UnityEngine;

public class RotateMe : MonoBehaviour
{
    public float RotationSpeed = 2.0f;
    public enum Axis
    {
        X, Y, Z,
        XY, XZ, YZ,
        XYZ
    };

    Vector3 targetRotation = Vector3.zero;
    
    public Axis axis;
    // Update is called once per frame
    void Update()
    {
        Vector3 dir = Vector3.zero;

        switch (axis)
        {
            case Axis.X:
                dir = new Vector3(1, 0, 0);
                break;
            case Axis.Y:
                dir = new Vector3(0, 1, 0);
                break;
            case Axis.Z:
                dir = new Vector3(0, 0, 1);
                break;
            case Axis.XY:
                dir = new Vector3(1, 1, 0);
                break;
            case Axis.XZ:
                dir = new Vector3(1, 0, 1);
                break;
            case Axis.YZ:
                dir = new Vector3(0, 1, 1);
                break;
            case Axis.XYZ:
                dir = new Vector3(1, 1, 1);
                break;
        }

        targetRotation += dir * RotationSpeed;
        var rotation = Quaternion.Euler(targetRotation);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 10.0f * Time.deltaTime);

    }
}
