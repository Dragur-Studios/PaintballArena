using Cinemachine.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class PlayerLocomotion : MonoBehaviour
{

    [SerializeField] Transform orientation;

    [Header("Physics Settings")]
    [SerializeField] LayerMask groundLayers;
    [SerializeField] LayerMask wallLayer;
    [SerializeField] float groundDrag = 4.0f;
    [SerializeField] float maxSlopAngle = 45.0f;

    [field: Header("Movement States")]
    [field: SerializeField, ReadOnly] public LocomotionState CurrentState { get; private set; }

    [Header("Walking")]
    [SerializeField] float MoveSpeed_Walk = 3.7f;


    [Header("Sprinting")]
    [SerializeField] float MoveSpeed_Sprint = 5.333f;

    [Header("Crouching")]
    [SerializeField] float MoveSpeed_Crouch = 2.0f;

 
    [Header("Jumping")]
    [SerializeField] float jumpForce = 5.0f;
    [SerializeField] float jumpCooldown = 2.0f;
    [SerializeField] float airMultiplier = 1.0f;
    [SerializeField, ReadOnly] bool isReadyForJump = false;

    [SerializeField] int allowedDoubleJumps = 1;
    [SerializeField, ReadOnly] int doubleJumpsCount = 0;

    [Header("Wall Running")]
    [SerializeField] float wallRunForce = 12.0f;
    [SerializeField] float maxWallRunTime = 1.5f;
    [SerializeField] float maxWallSpeed = 12.0f;
    [SerializeField] float maxWallRunCameraTilt = 30.0f;
    [SerializeField, ReadOnly] float currentWallRunCameraTilt;


    [Header("Sliding")]
    [SerializeField] float slideForce = 4.0f;

    // constants
    const float CrouchingHeight = 1.0f;
    const float StandingHeight = 2.0f;
    
    // state
    bool isWallRunning = false;
    bool isSprinting = false;
    bool isCrouching = false;
    bool isGrounded;


    // cache
    RaycastHit slopeHit;
    Vector3 normalVector = Vector3.zero;
    Vector2 moveDir = Vector2.zero;
    Vector3 moveDirection = Vector3.zero;
    bool isWallRight;
    bool isWallLeft;
    float moveSpeed;
    
    // refernces
    Animator anim;
    Rigidbody rigi;
    CapsuleCollider ccollider;
    PlayerInputDispatcher dispatcher;
    Player player;



    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<Player>();

        dispatcher = GetComponent<PlayerInputDispatcher>();
        dispatcher.OnMoveInputRecieved += OnMoveInputRecieved;
        dispatcher.OnCrouchInputRecieved += OnCrouchInputRecieved;
        dispatcher.OnSprintInputRecieved += OnSprintInputRecieved;
        dispatcher.OnJumpInputRecieved += OnJumpInputRecieved;
        
        SetCollisionHeight(StandingHeight);
        
        rigi = GetComponent<Rigidbody>();
        rigi.freezeRotation = true;
        ResetJump();

        anim = GetComponentInChildren<Animator>();
    }

    private void SetCollisionHeight(float height)
    {
        ccollider = GetComponent<CapsuleCollider>();
        ccollider.height = height;
        var center = ccollider.center;
        center.y = height * 0.5f;
        ccollider.center = center;
    }

    void OnMoveInputRecieved(Vector2 newDir)
    {
        if (!player.isAlive)
            return;
        if (player.isPaused)
            return;
        moveDir = newDir;
    }
    void OnCrouchInputRecieved(bool shouldCrouch)
    {
        if (!player.isAlive)
            return;
        if (player.isPaused)
            return;

        if (!isCrouching && shouldCrouch)
        {
            StartCrouch();
            isCrouching = true;
        }
        if(isCrouching && !shouldCrouch)
        {
            StopCrouch();
            isCrouching = false;
        }

    }
    void OnSprintInputRecieved(bool shouldSprint)
    {
        if (!player.isAlive)
            return;
        if (player.isPaused)
            return;
        isSprinting = shouldSprint;
    }

    void OnJumpInputRecieved(bool shouldJump)
    {
        if (!player.isAlive)
            return;
        if (player.isPaused)
            return;
        // double jump
        if (shouldJump && !isGrounded && doubleJumpsCount >= 1)
        {
            HandleJump();
            doubleJumpsCount--;
        }

        // normal jumps
        if (shouldJump && isReadyForJump && isGrounded)
            HandleJump();
    }

    private void Update()
    {
        if (!player.isAlive)
            return;
        if (player.isPaused)
            return;
        HandleGroundCheck();
        CheckForWalls();

        UpdateState();

        moveDirection = orientation.forward * moveDir.y + orientation.right * moveDir.x;

        ClampVelocity();

        if (isGrounded)
            rigi.drag = groundDrag;
        else
            rigi.drag = 0;

        transform.rotation = orientation.rotation;

    }

    void FixedUpdate()
    {
        if (!player.isAlive)
            return;
        if (player.isPaused)
            return;
        HandleMovement();
    }

    private void LateUpdate()
    {
        if (!player.isAlive)
            return;
        if (player.isPaused)
            return;

        anim.SetFloat("Velocity H", moveDir.x);
        anim.SetFloat("Velocity V", moveDir.y);

        anim.SetBool("isGrounded", isGrounded || isWallRunning);
        anim.SetBool("isWallRunning", isWallRunning);
        anim.SetBool("isWallLeft", isWallLeft);
        anim.SetBool("isWallRight", isWallRight);

    }



    bool IsOnSlope()
    {
        var currentHeight = isCrouching ? CrouchingHeight : StandingHeight;
        
        Vector3 origin = transform.position;
        origin.y += 0.5f;

        float dist = currentHeight + 0.2f;

        var dir = -orientation.transform.up;

        if (Physics.Raycast(origin, dir, out slopeHit, dist))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopAngle && angle != 0;
        }

        return false;
    }

    Vector3 GetSlopeMovementDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
    }

    void FindNormal(out Vector3 normal)
    {
        var currentHeight = isCrouching ? CrouchingHeight : StandingHeight;
        normal = Vector3.zero;

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, currentHeight * 0.5f + 0.2f, groundLayers))
        {
            normal = hit.normal;
        }

    }

    void UpdateState()
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

    public void StartCrouch()
    {
        SetCollisionHeight(CrouchingHeight);

        rigi.AddForce(Vector3.down * 5.0f, ForceMode.Impulse);

        var isCrouching = anim.GetBool("isCrouching");

        if (rigi.velocity.magnitude > 0.5f && !isCrouching)
        {
            if (isGrounded)
            {
                rigi.AddForce(orientation.transform.forward * slideForce, ForceMode.Impulse);
                rigi.AddForce(-orientation.transform.up * slideForce, ForceMode.Force);
                anim.SetTrigger("Slide");

                Invoke(nameof(StopCrouch), 1.0f);
            }
        }

        // handle as toggle
      
        if (isCrouching)
        {
            StopCrouch();
        }
        else
        {
            anim.SetBool("isCrouching", true);
        }
    }

    void StopCrouch()
    {
        anim.SetBool("isCrouching", false);
        SetCollisionHeight(StandingHeight);
    }

    void HandleCameraTilt()
    {

        if (Mathf.Abs(currentWallRunCameraTilt) < maxWallRunCameraTilt && isWallRunning && isWallRight)
        {
            currentWallRunCameraTilt += Time.deltaTime * maxWallRunCameraTilt * 20f;
        }
        if (Mathf.Abs(currentWallRunCameraTilt) < maxWallRunCameraTilt && isWallRunning && isWallLeft)
        {
            currentWallRunCameraTilt -= Time.deltaTime * maxWallRunCameraTilt * 20f;
        }

        if (currentWallRunCameraTilt > 0 && !isWallRight && !isWallLeft)
            currentWallRunCameraTilt -= Time.deltaTime * maxWallRunCameraTilt * 20f;
        if (currentWallRunCameraTilt < 0 && !isWallLeft && !isWallRight)
            currentWallRunCameraTilt += Time.deltaTime * maxWallRunCameraTilt * 20f;
        
        GetComponent<PlayerCameraController>().Tilt = currentWallRunCameraTilt;
    }


    void HandleMovement()
    {
        var gravityMultiplier = 10.0f;

        if (!isGrounded)
            rigi.AddForce(Vector3.down * Time.deltaTime * gravityMultiplier);

        if (IsOnSlope())
        {
            rigi.AddForce(GetSlopeMovementDirection() * moveSpeed * 10.0f, ForceMode.Force);
        }

        if (isGrounded)
            rigi.AddForce(moveDirection.normalized * moveSpeed * 10.0f, ForceMode.Force);
        else if (!isGrounded)
            rigi.AddForce(moveDirection.normalized * moveSpeed * 10.0f * airMultiplier, ForceMode.Force);

        if (isGrounded)
        {
            doubleJumpsCount = allowedDoubleJumps;
        }
    }

    //-----------------------------------------------------------
    // JUMP
    //-----------------------------------------------------------

    void HandleJump()
    {
        anim.CrossFade("Jump", 0.1f);

        // Handle Default Jump
        if (isGrounded)
        {
            isReadyForJump = false;

            rigi.AddForce(Vector3.up * jumpForce * 1.5f, ForceMode.Impulse);
            rigi.AddForce(normalVector * jumpForce * 0.5f, ForceMode.Impulse);

            var vel = rigi.velocity;
            if (rigi.velocity.y < 0.5f)
            {
                rigi.velocity = new Vector3(vel.x, 0, vel.z);
            }
            else if (rigi.velocity.y > 0)
            {
                rigi.velocity = new Vector3(vel.x, vel.y / 2, vel.z);
            }

            Invoke(nameof(ResetJump), jumpCooldown);

        }
        // Handle Double Jump
        else if (!isGrounded)
        {
            isReadyForJump = false;

            rigi.AddForce(orientation.forward * jumpForce * 1f, ForceMode.Impulse);
            rigi.AddForce(Vector3.up * jumpForce * 1.5f, ForceMode.Impulse);
            rigi.AddForce(normalVector * jumpForce * 0.5f, ForceMode.Impulse);

            rigi.velocity = Vector3.zero;

            Invoke(nameof(ResetJump), jumpCooldown);
        }
        // Handle WallRun Jump
        else if (isWallRunning)
        {
            isReadyForJump = false;

            bool isGoingLeft = moveDir.x < 0;
            bool isGoingRight = moveDir.x > 0;

            if (isWallLeft && !isGoingRight || isWallRight && !isGoingLeft)
            {
                rigi.AddForce(Vector3.up * jumpForce * 1.5f);
                rigi.AddForce(normalVector * jumpForce * 0.5f);
            }

            if (isWallRight || isWallLeft && isGoingLeft || isGoingRight)
                rigi.AddForce(-orientation.up * jumpForce * 1.0f);
            if (isWallRight && isGoingLeft) 
                rigi.AddForce(-orientation.right * jumpForce * 3.2f);
            if (isWallLeft && isGoingRight) 
                rigi.AddForce(orientation.right * jumpForce * 3.2f);

            rigi.AddForce(orientation.forward * jumpForce * 1.0f);

            rigi.velocity = Vector3.zero;

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }


    void ResetJump()
    {
        isReadyForJump = true;
    }

    //-----------------------------------------------------------
    // WALL RUN
    //-----------------------------------------------------------
    void StartWallRun()
    {
        rigi.useGravity = false;
        isWallRunning = true;

        if (rigi.velocity.magnitude <= maxWallSpeed)
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

        if (!isWallRight && !isWallLeft && isWallRunning)
            StopWallRunning();
    }



    //-----------------------------------------------------------
    // HELPERS 
    //-----------------------------------------------------------

    void ClampVelocity()
    {
        var flatVel = new Vector3(rigi.velocity.x, 0, rigi.velocity.z);

        if (flatVel.magnitude > moveSpeed)
        {
            var lim = flatVel.normalized * moveSpeed;
            rigi.velocity = new Vector3(lim.x, rigi.velocity.y, lim.z);
        }
    }

    private void HandleGroundCheck()
    {
        var currentHeight = isCrouching ? CrouchingHeight : StandingHeight;

        var origin = transform.position;
        origin.y += currentHeight * 0.5f;

        var dir = -orientation.up;
        var dist = currentHeight * 0.5f + 0.2f;
        isGrounded = Physics.Raycast(origin, dir, out RaycastHit hit, dist, groundLayers);

        Debug.DrawLine(origin, origin + dir * dist, isGrounded ? Color.green : Color.red, 0.1f);

        if (isGrounded)
        {
            normalVector = hit.normal;
            Debug.DrawLine(hit.point, hit.point + hit.normal, Color.blue, 0.1f);
        }
    }

    public void Die()
    {
        GameManager.SignalGameOver();

        ccollider.enabled = false;
        rigi.useGravity = false;

        anim.CrossFade("Death", 0.1f);
        Destroy(gameObject, 2.0f);

    }
}