using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class FollowPlayer : MonoBehaviour
{

    [SerializeField] Transform followMe;

    // Update is called once per frame

    private void Start()
    {
        transform.position = followMe.position;
        transform.rotation = Quaternion.identity;

    }
    void Update()
    {
        if(followMe != null)
        {
            transform.position = followMe.position;
        }
    }
}
