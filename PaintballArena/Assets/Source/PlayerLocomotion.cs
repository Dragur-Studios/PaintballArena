using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerLocomotion : MonoBehaviour
{
    [SerializeField] float MoveSpeed_Walk = 3.7f;
    [SerializeField] float MoveSpeed_Sprint = 5.333f;
    [SerializeField] float MoveSpeed_Crouch = 2.0f;
    float moveSpeed;

    [SerializeField] Transform orientation;

    Rigidbody rb;

    bool isGrounded;
    [SerializeField] float groundDrag = 4.0f;
    [SerializeField] float playerHeight;
    [SerializeField] LayerMask groundLayers;

    [SerializeField] float jumpForce = 5.0f;
    [SerializeField] float jumpCooldown = 2.0f;
    [SerializeField] float airMultiplier = 1.0f;
    [SerializeField] bool isReadyForJump = false;
    Vector3 normalVector = Vector3.zero;
    
    int doubleJumpsCount = 1;
    private int startDoubleJumps = 1;

    PlayerWallRun wallRun;
    Animator anim;

    Vector3 crouchScale = new Vector3(1.0f, 0.5f, 1.0f);
    Vector3 playerScale = Vector3.zero;

    [SerializeField] float slideForce = 4.0f;

    bool isSprinting = false;

    LocomotionState state;

    [Header("Slopes")]
    public float maxSlopAngle;
    RaycastHit slopeHit;

    Vector3 moveDirection = Vector3.zero;

    public enum LocomotionState
    {
        Walk,
        Crouch,
        Sprint,
        InAir
    }

    // Start is called before the first frame update
    void Start()
    {
        playerScale = transform.localScale;

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        ResetJump();
        wallRun = GetComponent<PlayerWallRun>();

        anim = GetComponentInChildren<Animator>();
    }

    public bool OnSlope()
    {
        Vector3 origin = transform.position;
        origin.y += 0.5f;

        float dist = playerHeight + 0.2f;

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
        normal = Vector3.zero;

        if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, playerHeight * 0.5f + 0.2f, groundLayers))
        {
            normal = hit.normal;
        }

    }

    void UpdateState()
    {
        if (Input.GetKey(KeyCode.C))
        {
            state = LocomotionState.Crouch;
            moveSpeed = MoveSpeed_Crouch;
        }


        if(isGrounded && Input.GetKey(KeyCode.LeftShift))
        {
            state = LocomotionState.Sprint;
            moveSpeed = MoveSpeed_Sprint;
        }
        else if (isGrounded)
        {
            state = LocomotionState.Walk;
            moveSpeed = MoveSpeed_Walk;
        }
        else
        {
            state = LocomotionState.InAir;
        }
    }

    private void LateUpdate()
    {
        var flatVel = new Vector3(moveDirection.x, 0, moveDirection.z);

        anim.SetFloat("Velocity", flatVel.normalized.magnitude * moveSpeed);
        anim.SetBool("isGrounded", isGrounded);
    }


    public void StartCrouch()
    {
        transform.localScale = crouchScale;
        rb.AddForce(Vector3.down * 5.0f, ForceMode.Impulse);

        if (rb.velocity.magnitude > 0.5f)
        {
            if (isGrounded)
            {
                rb.AddForce(orientation.transform.forward * slideForce, ForceMode.Impulse);
                anim.SetTrigger("Slide");

                Invoke(nameof(StopCrouch), 1.0f);
            }
        }

    }

    public void StopCrouch()
    {
        transform.localScale = playerScale;
    }

    private void Update()
    {
        UpdateState();

        HandleGroundCheck();

        var vertical = Input.GetAxisRaw("Vertical");
        var horizontal = Input.GetAxisRaw("Horizontal");
        //moveDir = new Vector2(Input.GetAxisRaw("Vertical"), Input.GetAxisRaw("Vertical"));
        moveDirection = orientation.forward * vertical + orientation.right * horizontal;

        if (Input.GetKeyDown(KeyCode.C))
        {
            StartCrouch();
        }
        if (Input.GetKeyUp(KeyCode.C))
        {
            StopCrouch();
        }

        isSprinting = Input.GetKey(KeyCode.LeftShift);

        // double jump
        if (Input.GetKeyDown(KeyCode.Space) && !isGrounded && doubleJumpsCount >= 1)
        {
            HandleJump();
            doubleJumpsCount--;
        }

        // normal jumps
        if (Input.GetKeyDown(KeyCode.Space) && isReadyForJump && isGrounded)
            HandleJump();

        ClampVelocity();

        if (isGrounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;

       

        transform.rotation = orientation.rotation;



    }

    private void HandleGroundCheck()
    {
        var origin = transform.position;
        origin.y += playerHeight * 0.5f;

        var dir = -orientation.up;
        var dist = playerHeight * 0.5f + 0.2f;
        isGrounded = Physics.Raycast(origin, dir, out RaycastHit hit, dist, groundLayers);
        
        Debug.DrawLine(origin, origin + dir * dist, isGrounded ? Color.green : Color.red, 0.1f);
     
        if (isGrounded)
        {
            normalVector = hit.normal;
            Debug.DrawLine(hit.point, hit.point + hit.normal, Color.blue, 0.1f);
        }
    }



    // Update is called once per frame
    void FixedUpdate()
    {
        HandleMovement();

    }

    private void HandleMovement()
    {
        var gravityMultiplier = 10.0f;

        if (!isGrounded)
            rb.AddForce(Vector3.down * Time.deltaTime * gravityMultiplier);

        if (OnSlope())
        {
            rb.AddForce(GetSlopeMovementDirection() * moveSpeed * 10.0f, ForceMode.Force);
        }

        if (isGrounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10.0f, ForceMode.Force);
        else if (!isGrounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10.0f * airMultiplier, ForceMode.Force);

        if (isGrounded)
        {
            doubleJumpsCount = startDoubleJumps;
        }
    }
    private void HandleJump()
    {
        anim.CrossFade("Jump", 0.1f);

        // Handle Default Jump
        if (isGrounded)
        {
            isReadyForJump = false;

            rb.AddForce(Vector3.up * jumpForce * 1.5f, ForceMode.Impulse);
            rb.AddForce(normalVector * jumpForce * 0.5f, ForceMode.Impulse);

            var vel = rb.velocity;
            if (rb.velocity.y < 0.5f)
            {
                rb.velocity = new Vector3(vel.x, 0, vel.z);
            }
            else if (rb.velocity.y > 0)
            {
                rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);
            }

            Invoke(nameof(ResetJump), jumpCooldown);

        }
        // Handle Double Jump
        else if (!isGrounded)
        {
            isReadyForJump = false;

            rb.AddForce(orientation.forward * jumpForce * 1f, ForceMode.Impulse);
            rb.AddForce(Vector3.up * jumpForce * 1.5f, ForceMode.Impulse);
            rb.AddForce(normalVector * jumpForce * 0.5f, ForceMode.Impulse);

            //rb.velocity = Vector3.zero;

            Invoke(nameof(ResetJump), jumpCooldown);
        }
        // Handle WallRun Jump
        else if (wallRun.isWallRunning)
        {
            isReadyForJump = false;

            if (wallRun.isWallLeft && !Input.GetKey(KeyCode.D) || wallRun.isWallRight && !Input.GetKey(KeyCode.A))
            {
                rb.AddForce(Vector3.up * jumpForce * 1.5f);
                rb.AddForce(normalVector * jumpForce * 0.5f);
            }

            if (wallRun.isWallRight || wallRun.isWallLeft && Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
                rb.AddForce(-orientation.up * jumpForce * 1.0f);
            if (wallRun.isWallRight && Input.GetKey(KeyCode.A)) rb.AddForce(-orientation.right * jumpForce * 3.2f);
            if (wallRun.isWallLeft && Input.GetKey(KeyCode.D)) rb.AddForce(orientation.right * jumpForce * 3.2f);

            rb.AddForce(orientation.forward * jumpForce * 1.0f);

            //rb.velocity = Vector3.zero;

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }
    void ClampVelocity()
    {
        var flatVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        if(flatVel.magnitude > moveSpeed)
        {
            var lim = flatVel.normalized * moveSpeed; 
            rb.velocity = new Vector3(lim.x, rb.velocity.y, lim.z); 
        }
    }

    
    void ResetJump()
    {
        isReadyForJump = true;
    }
}
