using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class CameraRig : MonoBehaviour
{

    [SerializeField] Transform followTarget;

    // Update is called once per frame

    public void StartFolow(Transform target)
    {
        followTarget = target; 
        transform.position = target.position;
        transform.rotation = Quaternion.identity;

    }
    void Update()
    {
        if(followTarget != null)
        {
            transform.position = followTarget.position;
        }
    }
}
