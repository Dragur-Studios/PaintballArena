using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class Enemy : iGameStateListener
{
    [field: SerializeField] public Transform Target { get; set; }


    public bool isGameStarted { get; private set; } = false;
    public bool isAlive { get; private set; } = true;
    public bool isGamePaused { get; private set; } = false;
    public bool isGameOver { get; private set; } = false;

    public void Die()
    {
        if (Target != null)
            Target = null;

        isAlive = false;

        GetComponent<CapsuleCollider>().enabled = false;
        GetComponent<EnemyLocomotion>().enabled = false;

        //agent.SetDestination(transform.position);

        GetComponent<HumanoidAnimatorController>().Crossfade("Death");
        
        GameManager.RemoveEnemyFromActvePool(this);

        Destroy(gameObject, 5.0f);

    }

    public override void HandleGameOver()
    {
        isGameOver = true;
        if (Target != null)
        {
            Target = null;
        }
        
        GetComponent<HumanoidLocomotion>().enabled = false;
    }

    public override void HandleGamePaused()
    {
        isGameStarted = true;
    }

    public override void HandleGameUnpaused()
    {
        isGameStarted = false;
    }

    public override void HangleGameStarted()
    {
        isGameStarted = true;
    }
}
