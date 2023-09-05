
using System.Collections;
using System.Collections.Generic;


using UnityEngine;


public class PlayerLocomotion : HumanoidLocomotion
{
    public Transform orientation;

    // refernces
    HumanoidAnimatorController animator;
    Rigidbody rigi;
    CapsuleCollider ccollider;
    PlayerInputDispatcher dispatcher;
    Player player;

    // Start is called before the first frame update
    public override void Init()
    {
        player = GetComponent<Player>();

        dispatcher = GetComponent<PlayerInputDispatcher>();
        dispatcher.OnMoveInputRecieved += OnMoveInputRecieved;
        dispatcher.OnCrouchInputRecieved += OnCrouchInputRecieved;
        dispatcher.OnSprintInputRecieved += OnSprintInputRecieved;
        dispatcher.OnJumpInputRecieved += OnJumpInputRecieved;
        

        ccollider = GetComponent<CapsuleCollider>();
        SetColliderHeight(ref ccollider, StandingHeight);
        
        rigi = GetComponent<Rigidbody>();
        rigi.freezeRotation = true;
        ResetJump();

        animator = GetComponentInChildren<HumanoidAnimatorController>();
    }

    void OnMoveInputRecieved(Vector2 newDir)
    {
    
        moveDir = newDir;
    }
    void OnCrouchInputRecieved(bool shouldCrouch)
    {
        
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
      
        isSprinting = shouldSprint;
    }

    void OnJumpInputRecieved(bool shouldJump)
    {
       
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

    public override void StateUpdate()
    {   
        if(!player.isGameStarted) return;
        if (!player.isAlive) return;
        if (player.isPaused) return;

        HandleGroundCheck();
        CheckForWalls(transform.position, orientation, wallLayer, out isWallLeft, out isWallRight);
        UpdateState();

        moveDirection = orientation.forward * moveDir.y + orientation.right * moveDir.x;

        ClampVelocity();

        if (isGrounded)
            rigi.drag = groundDrag;
        else
            rigi.drag = 0;

        transform.rotation = orientation.rotation;

    }

    public override void PhysicsUpdate()
    {
        if (!player.isGameStarted) return;
        if (!player.isAlive) return;
        if (player.isPaused) return;

        HandleMovement();
    }

    public override void AnimatorUpdate()
    {
        if (!player.isGameStarted) return;
        if (!player.isAlive) return;
        if (player.isPaused) return;

        animator.SetFloat("Velocity H", moveDir.x);
        animator.SetFloat("Velocity V", moveDir.y);

        animator.SetBool("isGrounded", isGrounded || isWallRunning);
        animator.SetBool("isWallRunning", isWallRunning);
        animator.SetBool("isWallLeft", isWallLeft);
        animator.SetBool("isWallRight", isWallRight);

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
        SetColliderHeight(ref ccollider, CrouchingHeight);

        rigi.AddForce(Vector3.down * 5.0f, ForceMode.Impulse);


        if (rigi.velocity.magnitude > 0.5f && !isCrouching)
        {
            if (isGrounded)
            {
                rigi.AddForce(orientation.transform.forward * slideForce, ForceMode.Impulse);
                rigi.AddForce(-orientation.transform.up * slideForce, ForceMode.Force);
                animator.Trigger("Slide");

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
            animator.SetBool("isCrouching", true);
        }
    }

    void StopCrouch()
    {
        animator.SetBool("isCrouching", false);
        SetColliderHeight( ref ccollider , StandingHeight);
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

        if (IsOnSlope(orientation))
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

        if (isWallRunning)
        {
            if (!isWallRight && !isWallLeft)
                StopWallRunning();
        }
    }

    //-----------------------------------------------------------
    // JUMP
    //-----------------------------------------------------------

    void HandleJump()
    {
        animator.Crossfade("Jump");

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

    //private void HandleGroundCheck()
    //{
    //    var currentHeight = isCrouching ? CrouchingHeight : StandingHeight;

    //    var origin = transform.position;
    //    origin.y += currentHeight * 0.5f;

    //    var dir = -orientation.up;
    //    var dist = currentHeight * 0.5f + 0.2f;
    //    isGrounded = Physics.Raycast(origin, dir, out RaycastHit hit, dist, groundLayers);

    //    Debug.DrawLine(origin, origin + dir * dist, isGrounded ? Color.green : Color.red, 0.1f);

    //    if (isGrounded)
    //    {
    //        normalVector = hit.normal;
    //        Debug.DrawLine(hit.point, hit.point + hit.normal, Color.blue, 0.1f);
    //    }
    //}


}