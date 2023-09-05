using UnityEngine.Animations.Rigging;
using UnityEngine;

public class Player : iGameStateListener
{
    public bool isGameStarted { get; private set; } = false;
    public bool isPaused { get; private set; } = false;
    public bool isAlive { get; private set; } = true;

    public Transform CameraFollowTarget;
    
    PlayerLocomotion locomotion;
    HumanoidAnimatorController animator;

    protected override void Start()
    {
        base.Start();
        locomotion = GetComponent<PlayerLocomotion>();
        animator = GetComponent<HumanoidAnimatorController>();
    }


    public void Die()
    {
        isAlive = false;

        locomotion.enabled = false;

        //ccollider.enabled = false;
        //rigi.useGravity = false;
        animator.Crossfade("Death");
        
        Destroy(gameObject, 2.0f);

    }


    public override void HandleGameOver()
    {
        isAlive = false;
    }

    public override void HandleGamePaused()
    {
        isPaused = true;
    }

    public override void HandleGameUnpaused()
    {
        isPaused = false;
    }

    public override void HangleGameStarted()
    {
        isGameStarted = true;
    }
}
