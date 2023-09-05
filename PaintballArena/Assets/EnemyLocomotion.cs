using UnityEngine;
using UnityEngine.AI;

public class EnemyLocomotion : HumanoidLocomotion
{
    [field: SerializeField, ReadOnly] AIBehaviorState BehaviorState { get; set; } 

    HumanoidAnimatorController anim;
    Vector3 waypoint = Vector3.zero;
    Enemy enemy;
    NavMeshAgent agent;

    EnemyWeaponHandler weaponHandler;

    public override void Init()
    {
        enemy = GetComponent<Enemy>();

        weaponHandler = GetComponent<EnemyWeaponHandler>();

        waypoint = transform.position;

        anim = GetComponent<HumanoidAnimatorController>();


        agent = GetComponent<NavMeshAgent>();


        // TEMPORARY. AUTOMATICALLY SETS THE TARGET TO SEARCH FOR PLAYER

        BehaviorState = AIBehaviorState.SeekAndKill;

    }
  

    public override void StateUpdate()
    {
        if (enemy.isGameOver || enemy.isGamePaused || !enemy.isGameStarted)
            return;

        HandleGroundCheck();

        UpdateLocomotionState();


        UpdateBehaviorState();


        HandleMovement();

    }

    private void HandleMovement()
    {
        if (BehaviorState == AIBehaviorState.SeekAndKill)
        {
            var dist = Mathf.Abs((enemy.Target.position - transform.position).magnitude);
            if (dist < weaponHandler.maxAttackRange)
            {
                BehaviorState = AIBehaviorState.Attack;
            }
            if (enemy.Target != null)
            {
                agent.speed = moveSpeed;
                agent.stoppingDistance = weaponHandler.minAttackRange;
                agent.SetDestination(enemy.Target.position);
            }
        }
        
        if (BehaviorState == AIBehaviorState.Patrol)
        {
            //if (enemy.Target != null)
            //{
            //    BehaviorState = AIBehaviorState.SeekAndKill;
            //    return;
            //}

            if (waypoint != null)
            {
                agent.speed = moveSpeed;
                agent.stoppingDistance = 1;
                agent.SetDestination(waypoint);
            }
        }
        
        if (BehaviorState == AIBehaviorState.Attack)
        {
            if (enemy.Target != null)
            {
                // continue to follow the target if they move too far.
                //var dist = Mathf.Abs((enemy.Target.position - transform.position).magnitude);
                //if (dist > weaponHandler.maxAttackRange * 1.5f)
                //{
                //    BehaviorState = AIBehaviorState.SeekAndKill;
                //    return;
                //}
                // stop moving the agent
                agent.stoppingDistance = 2.0f;
                agent.SetDestination(enemy.Target.position);
                AttackPlayer();

            }
        }

        //else
        //{
        //    // stop moving the agent because we are in an undefined state
        //    agent.SetDestination(transform.position);
        //}

        moveDir = agent.velocity;
    }

    public override void PhysicsUpdate()
    {
        if (enemy.isGameOver || enemy.isGamePaused || !enemy.isGameStarted)
            return;




    }

    public override void AnimatorUpdate()
    {
        if (enemy.isGameOver || enemy.isGamePaused || !enemy.isGameStarted)
            return;

        anim.SetFloat("Velocity H", agent.velocity.x);
        anim.SetFloat("Velocity V", agent.velocity.z);

        anim.SetBool("isGrounded", isGrounded || isWallRunning);
        anim.SetBool("isWallRunning", isWallRunning);
        anim.SetBool("isWallLeft", isWallLeft);
        anim.SetBool("isWallRight", isWallRight);
    }

    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        Vector3 randomPoint = center + UnityEngine.Random.insideUnitSphere * range;
        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    void UpdateBehaviorState()
    {
        


        switch (BehaviorState)
        {
            case AIBehaviorState.Patrol:
                // FInd A random position within a certian radius thats valid and move their.
                {


                    if (waypoint == transform.position)
                    {
                        if (!RandomPoint(transform.position, 10.0f, out waypoint))
                        {
                            return;
                        }
                    }

                    var dist = Mathf.Abs((enemy.Target.position - waypoint).magnitude);

                    if (dist > 5.0f)
                    {
                        moveSpeed = MoveSpeed_Sprint;
                    }
                    else if (dist > 1.0f)
                    {
                        moveSpeed = MoveSpeed_Walk;
                    }
                    else
                    {
                        waypoint = transform.position;
                    }

                }
                break;
            case AIBehaviorState.SeekAndKill:
                {
                    if (enemy.Target == null)
                        return;
                    var dist = Mathf.Abs((enemy.Target.position - transform.position).magnitude);

                    if (dist > 5.0f)
                    {
                        moveSpeed = MoveSpeed_Sprint;
                    }
                    else
                    {
                        moveSpeed = MoveSpeed_Walk;
                        BehaviorState = AIBehaviorState.Attack;
                    }

                }
                break;
            case AIBehaviorState.Attack:
                {
                    if (enemy.Target == null)
                        return;
                    var dist = Mathf.Abs((enemy.Target.position - transform.position).magnitude);
                    if (dist < weaponHandler.minAttackRange)
                    {
                        // need to move... find a more tactical spot. 

                        moveSpeed = MoveSpeed_Walk;
                        //moveSpeed = MoveSpeed_Sprint;
                    }
                    else if (dist > weaponHandler.maxAttackRange)
                    {
                        moveSpeed = MoveSpeed_Sprint;
                        BehaviorState = AIBehaviorState.SeekAndKill;
                    }
                    else
                    {
                        // try to attack the player
                        moveSpeed = MoveSpeed_Walk;

                    }

                }
                break;
        }
    }

    void UpdateLocomotionState()
    {
        bool isGoingLeft = moveDir.x < 0;
        bool isGoingRight = moveDir.x > 0;

        if (isGrounded && isSprinting)
        {
            CurrentState = LocomotionState.Sprint;
            moveSpeed = MoveSpeed_Sprint;
        }
        else if (isGrounded && isCrouching)
        {
            CurrentState = LocomotionState.Crouch;
            moveSpeed = MoveSpeed_Crouch;
        }
        else if (isGrounded)
        {
            CurrentState = LocomotionState.Walk;
            moveSpeed = MoveSpeed_Walk;
        }
        else if (!isGrounded && ((isGoingRight && isWallRight) || (isGoingLeft && isWallLeft)))
        {
            CurrentState = LocomotionState.WallRun;

        }
        else
        {
            CurrentState = LocomotionState.InAir;
        }




    }


    
    void AttackPlayer()
    {
        

        weaponHandler.AimAndFire();
    }


    void OnAnimatorIK(int layerIndex)
    {
        
        float weight = weaponHandler.CurrentWeapon != null && enemy.isAlive ? 1.0f : 0.0f;

        if (weaponHandler.CurrentWeapon != null)
        {
            //RIGHT HAND IK 
            anim.SetIKPositionWeight(AvatarIKGoal.RightHand, weight);
            anim.SetIKRotationWeight(AvatarIKGoal.RightHand, weight);
            anim.SetIKHintPositionWeight(AvatarIKHint.RightElbow, weight);

            anim.SetIKHintPosition(AvatarIKHint.RightElbow, weaponHandler.CurrentWeapon.RightHandIKHint.position);
            anim.SetIKPosition(AvatarIKGoal.RightHand, weaponHandler.CurrentWeapon.RightHandIK.position);
            anim.SetIKRotation(AvatarIKGoal.RightHand, weaponHandler.CurrentWeapon.RightHandIK.rotation);

            //LEFT HAND IK 
            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, weight);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, weight);
            anim.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, weight);
            anim.SetIKHintPosition(AvatarIKHint.LeftElbow, weaponHandler.CurrentWeapon.LeftHandIKHint.position);
            anim.SetIKPosition(AvatarIKGoal.LeftHand, weaponHandler.CurrentWeapon.LeftHandIK.position);
            anim.SetIKRotation(AvatarIKGoal.LeftHand, weaponHandler.CurrentWeapon.LeftHandIK.rotation);
        }
    }

}
