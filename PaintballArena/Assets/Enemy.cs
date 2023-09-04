using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;


public enum AIBehaviorState
{
    Patrol,
    Follow,
    Attack
}

public class Enemy : MonoBehaviour
{

    [field: SerializeField] public Transform Target { get; set; }
    [Header("Walking")]
    [SerializeField] float MoveSpeed_Walk = 3.7f;
    [SerializeField] float MoveSpeed_WalkAiming = 1f;


    [Header("Sprinting")]
    [SerializeField] float MoveSpeed_Sprint = 5.333f;

    [Header("Crouching")]
    [SerializeField] float MoveSpeed_Crouch = 2.0f;
    NavMeshAgent agent;
    Animator anim;
    [field: SerializeField] public EnemyEquippedWeapon CurrentWeapon { get; private set; }

    [SerializeField, ReadOnly] AIBehaviorState BehaviorState;
    [SerializeField, ReadOnly] LocomotionState CurrentState;
    [SerializeField, ReadOnly] EquippedWeaponState WeaponState;

    [SerializeField] LayerMask groundLayers;
    [SerializeField] LayerMask wallLayer;
    // constants 
    const float CrouchingHeight = 1.0f;
    const float StandingHeight = 2.0f;
    // state
    bool isWallRunning = false;
    bool isSprinting = false;
    bool isCrouching = false;
    bool isGrounded;
    bool isWallLeft = false;
    bool isWallRight = false;

    float moveSpeed;

    Vector3 normalVector = Vector3.zero;
    Vector3 moveDir = Vector3.zero;


    [Header("Poses")]
    [SerializeField] Transform aimDownSightPose;
    [SerializeField] Transform idlePose;
    [SerializeField] Transform runPose;
    [SerializeField] Transform crouchPose;

    Vector3 weaponPosition = Vector3.zero;
    bool isAiming = false;


    [Header("Attack Behavior Settings")]
    [SerializeField] float minAttackRange = 2.0f;
    [SerializeField] float maxAttackRange = 10.0f;
    Vector3 waypoint = Vector3.zero;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minAttackRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxAttackRange);
    }

    private void Start()
    {

        waypoint = transform.position;

        anim = GetComponent<Animator>();

        Target = FindObjectOfType<PlayerLocomotion>().transform;

        agent = GetComponent<NavMeshAgent>();
        
        // this may be used to stop for firing.
        agent.stoppingDistance = minAttackRange;
        BehaviorState = AIBehaviorState.Follow;
        agent.SetDestination(Target.position);

    }

    void UpdateWeaponPosition()
    {
        switch (WeaponState)
        {
            case EquippedWeaponState.Idle:
                {
                    weaponPosition = idlePose.position;
                }
                break;
            case EquippedWeaponState.Run:
                {
                    weaponPosition = runPose.position;
                }
                break;
            case EquippedWeaponState.Crouch:
                {
                    weaponPosition = crouchPose.position;
                }
                break;
            case EquippedWeaponState.AimDownSight:
                {
                    weaponPosition = aimDownSightPose.position;
                }
                break;
        }
    }

    private void HandleGroundCheck()
    {
        var currentHeight = isCrouching ? CrouchingHeight : StandingHeight;

        var origin = transform.position;
        origin.y += currentHeight * 0.5f;

        var dir = -transform.up;
        var dist = currentHeight * 0.5f + 0.2f;
        isGrounded = Physics.Raycast(origin, dir, out RaycastHit hit, dist, groundLayers);

        Debug.DrawLine(origin, origin + dir * dist, isGrounded ? Color.green : Color.red, 0.1f);

        if (isGrounded)
        {
            normalVector = hit.normal;
            Debug.DrawLine(hit.point, hit.point + hit.normal, Color.blue, 0.1f);
        }
    }

    private void LateUpdate()
    {

        anim.SetFloat("Velocity H", agent.velocity.x);
        anim.SetFloat("Velocity V", agent.velocity.z);

        anim.SetBool("isGrounded", isGrounded || isWallRunning);
        anim.SetBool("isWallRunning", isWallRunning);
        anim.SetBool("isWallLeft", isWallLeft);
        anim.SetBool("isWallRight", isWallRight);
        anim.SetBool("isAiming", isAiming);


    }

    bool RandomPoint(Vector3 center, float range, out Vector3 result) 
    {
        Vector3 randomPoint = center + UnityEngine.Random.insideUnitSphere * range;
        if(NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
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

                    var dist = Mathf.Abs((Target.position - waypoint).magnitude);

                    if (dist > 5.0f)
                    {
                        moveSpeed = MoveSpeed_Sprint;
                    }
                    else if(dist > 1.0f)
                    {
                        moveSpeed = MoveSpeed_Walk;
                    }
                    else
                    {
                        waypoint = transform.position;
                    }
                    
                }
                break;
            case AIBehaviorState.Follow:
                {
                    var dist = Mathf.Abs((Target.position - transform.position).magnitude);
                    
                    if(dist > 5.0f)
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
                    
                    var dist = Mathf.Abs((Target.position - transform.position).magnitude);
                    if (dist < minAttackRange)
                    {
                        // need to move... find a more tactical spot. 

                        moveSpeed = MoveSpeed_Walk;
                        //moveSpeed = MoveSpeed_Sprint;
                    }
                    else if(dist > maxAttackRange)
                    {
                        moveSpeed = MoveSpeed_Sprint;
                        BehaviorState = AIBehaviorState.Follow;
                    }
                    else
                    {
                        // try to attack the player
                        moveSpeed = MoveSpeed_WalkAiming;

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


    void UpdateWeaponState()
    {
        var isSprinting = CurrentState == LocomotionState.Sprint;
        var isCrouching = CurrentState == LocomotionState.Crouch;

        var isReloading = CurrentWeapon.isReloading;
        var isOutOfAmmo = CurrentWeapon.isOutOfAmmo;

        if (isSprinting)
        {
            WeaponState = EquippedWeaponState.Run;
        }
        else if (isCrouching)
        {
            WeaponState = EquippedWeaponState.Crouch;
        }
        else if (isAiming)
        {
            WeaponState = EquippedWeaponState.AimDownSight;
        }
        else if (!isAiming && !isSprinting)
        {
            WeaponState = EquippedWeaponState.Idle;
        }
    }


    private void Update()
    {
        HandleGroundCheck();

        UpdateLocomotionState();
        UpdateWeaponState();
        UpdateWeaponPosition();

        UpdateBehaviorState();


        // handle movement 

        if (BehaviorState == AIBehaviorState.Follow)
        {
            if(Target != null)
            {
                agent.speed = moveSpeed;
                agent.stoppingDistance = minAttackRange;
                agent.SetDestination(Target.position);
            }
        }
        else if(BehaviorState == AIBehaviorState.Patrol)
        {
            if(waypoint != null)
            {
                agent.speed = moveSpeed;
                agent.stoppingDistance = 1;
                agent.SetDestination(waypoint);
            }
        }
        else if(BehaviorState == AIBehaviorState.Attack)
        {
            if(Target != null)
            {
                // continue to follow the target if they move too far.
                var dist = Mathf.Abs((Target.position - transform.position).magnitude);
                if(dist > maxAttackRange)
                {
                    BehaviorState = AIBehaviorState.Follow;
                    return;
                }
                // stop moving the agent
                agent.stoppingDistance = minAttackRange;
                agent.SetDestination(Target.position);
                AttackPlayer();

            }
        }
        else
        {
            // stop moving the agent because we are in an undefined state
            agent.SetDestination(transform.position);
        }

        moveDir = agent.velocity;

    }

    private void AttackPlayer()
    {
        var dir = (Target.position - transform.position);
        dir.y = 0;
        dir.Normalize();

        var lookdir = Quaternion.LookRotation(dir, transform.up);
        transform.rotation = lookdir;

        isAiming = true;
        
        CurrentWeapon?.PullTrigger();

    }

    void OnAnimatorIK(int layerIndex)
    {
        if (CurrentWeapon != null)
        {
            //RIGHT HAND IK 
            anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
            anim.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 1);

            anim.SetIKHintPosition(AvatarIKHint.RightElbow, CurrentWeapon.RightHandIKHint.position);
            anim.SetIKPosition(AvatarIKGoal.RightHand, CurrentWeapon.RightHandIK.position);
            anim.SetIKRotation(AvatarIKGoal.RightHand, CurrentWeapon.RightHandIK.rotation);

            //LEFT HAND IK 
            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
            anim.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 1);
            anim.SetIKHintPosition(AvatarIKHint.LeftElbow, CurrentWeapon.LeftHandIKHint.position);
            anim.SetIKPosition(AvatarIKGoal.LeftHand, CurrentWeapon.LeftHandIK.position);
            anim.SetIKRotation(AvatarIKGoal.LeftHand, CurrentWeapon.LeftHandIK.rotation);
        }
    }

}
