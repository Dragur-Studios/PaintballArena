using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetPlayer : MonoBehaviour
{

    [SerializeField] Transform spawnPoint;
    Transform player;
    
    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerLocomotion>().transform;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            player.position = spawnPoint.position;
        }
    }
}
