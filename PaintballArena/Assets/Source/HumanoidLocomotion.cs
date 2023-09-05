using System;
using UnityEngine;

public abstract class HumanoidLocomotion : MonoBehaviour
{
    [Header("Physics Settings")]
    [SerializeField] protected LayerMask groundLayers;
    [SerializeField] protected LayerMask wallLayer;
    [SerializeField] protected float groundDrag = 4.0f;
    [SerializeField] protected float maxSlopAngle = 45.0f;

    [field: Header("Movement States")]
    [field: SerializeField, ReadOnly] public LocomotionState CurrentState { get; protected set; }

    [Header("Walking")]
    [SerializeField] protected float MoveSpeed_Walk = 3.7f;

    [Header("Sprinting")]
    [SerializeField] protected float MoveSpeed_Sprint = 5.333f;

    [Header("Crouching")]
    [SerializeField] protected float MoveSpeed_Crouch = 2.0f;


    [Header("Jumping")]
    [SerializeField] protected float jumpForce = 5.0f;
    [SerializeField] protected float jumpCooldown = 2.0f;
    [SerializeField] protected float airMultiplier = 1.0f;

    [SerializeField] protected int allowedDoubleJumps = 1;
    [SerializeField, ReadOnly] protected int doubleJumpsCount = 0;

    [Header("Wall Running")]
    [SerializeField] protected float wallRunForce = 12.0f;
    [SerializeField] protected float maxWallRunTime = 1.5f;
    [SerializeField] protected float maxWallSpeed = 12.0f;
    [SerializeField] protected float maxWallRunCameraTilt = 30.0f;
    [SerializeField, ReadOnly] protected float currentWallRunCameraTilt;


    [Header("Sliding")]
    [SerializeField] protected float slideForce = 4.0f;

    // state
    protected bool isReadyForJump = false;
    protected bool isWallRunning = false;
    protected bool isSprinting = false;
    protected bool isCrouching = false;
    protected bool isGrounded;
    
    // cache
    RaycastHit slopeHit;
    protected Vector3 moveDirection = Vector3.zero;
    protected Vector3 normalVector = Vector3.zero;
    protected Vector2 moveDir = Vector2.zero;
    protected bool isWallRight;
    protected bool isWallLeft;
    protected float moveSpeed;


    // constants
    public const float CrouchingHeight = 1.0f;
    public const float StandingHeight = 2.0f;

    public Action OnInitillize;
    public Action OnPhysicsUpdate;
    public Action OnUpdate;
    public Action OnAnimatorUpdate;

    private void Awake()
    {
        OnInitillize += Init;

        OnUpdate = StateUpdate;
        OnPhysicsUpdate += PhysicsUpdate;
        OnAnimatorUpdate += AnimatorUpdate;
    }

    public abstract void Init();
    public abstract void PhysicsUpdate();
    public abstract void AnimatorUpdate();
    public abstract void StateUpdate();

    private void Start()
    {
        OnInitillize?.Invoke();
    }
    private void FixedUpdate()
    {
        OnPhysicsUpdate?.Invoke();
    }
    private void Update()
    {
        OnUpdate?.Invoke();
    }
    private void LateUpdate()
    {
        OnAnimatorUpdate?.Invoke();
    }

    protected static void SetColliderHeight(ref CapsuleCollider col, float height)
    {
        col.height = height;
        var center = col.center;
        center.y = height * 0.5f;
        col.center = center;
    }
    protected static void CheckForWalls(Vector3 origin, Transform orientation, LayerMask wallLayer, out bool isWallLeft, out bool isWallRight)
    {
        isWallRight = Physics.Raycast(origin, orientation.right, 1.0f, wallLayer);
        isWallLeft = Physics.Raycast(origin, -orientation.right, 1.0f, wallLayer);
    }
    protected static void FindNormal(Vector3 position, float height, LayerMask contactLayers, out Vector3 normal)
    {
        normal = Vector3.zero;

        if (Physics.Raycast(position, Vector3.down, out RaycastHit hit, height * 0.5f + 0.2f, contactLayers))
        {
            normal = hit.normal;
        }

    }

    protected bool IsOnSlope(Transform orientation)
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

    protected Vector3 GetSlopeMovementDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
    }
    protected void HandleGroundCheck()
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



}
