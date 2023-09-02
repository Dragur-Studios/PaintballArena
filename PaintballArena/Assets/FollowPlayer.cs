using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class FollowPlayer : MonoBehaviour
{

    [SerializeField] Transform followMe;

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = followMe.position;
    }
}
