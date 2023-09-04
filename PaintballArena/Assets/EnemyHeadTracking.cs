using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHeadTracking : MonoBehaviour
{
    [SerializeField] Transform lookTarget;

    Enemy enemy;

    private void Start()
    {
        enemy = GetComponent<Enemy>();
    }

    private void Update()
    {
        lookTarget.position = new Vector3(enemy.Target.position.x, enemy.Target.position.y + 1.5f, enemy.Target.position.z);

    }
}
