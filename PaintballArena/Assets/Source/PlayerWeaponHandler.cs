using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum EquippedWeaponState
{
    Idle,
    Run,
    Crouch,
    AimDownSight,
}

public class PlayerWeaponHandler : MonoBehaviour
{
    public Action OnAmmoChanged;

    [Header("Poses")]
    [SerializeField] Transform aimDownSightPose;
    [SerializeField] Transform idlePose;
    [SerializeField] Transform runPose;
    [SerializeField] Transform crouchPose;

    [Header("Cache")]
    [SerializeField, ReadOnly] EquippedWeaponState state;
    [SerializeField, ReadOnly] bool isAiming = false;
    
    [field: SerializeField] public EquippedWeapon CurrentWeapon { get; private set; }

    Animator anim;
    Vector3 targetPosition = Vector3.zero;

    Player player;
    PlayerLocomotion locomotion;
    PlayerInputDispatcher dispatcher;




  
    private void Start()
    {
        dispatcher = GetComponent<PlayerInputDispatcher>();
        dispatcher.OnFireInputHeld += OnFireInputRecieved;
        dispatcher.OnAimInputRecieved += OnAimInputRecieved;
        dispatcher.OnReloadInputRecieved += OnReloadInputRecieved;

        player = GetComponent<Player>();
        locomotion = GetComponent<PlayerLocomotion>();

        anim = GetComponent<Animator>();

        //OnAmmoChanged += UpdateCurrentAmmoText;


    }

    void Update()
    {
        if (!player.isAlive)
            return;
        if (player.isPaused)
            return;

        UpdateState();

        UpdateWeaponPosition();
    }
    void FixedUpdate()
    {
        if (!player.isAlive)
            return;
        if (player.isPaused)
            return;

        var cur = CurrentWeapon.transform.position;
        CurrentWeapon.transform.position = Vector3.Lerp(cur, targetPosition, 10.0f * Time.deltaTime);
    }
    void OnAnimatorIK(int layerIndex)
    {
        float weight = CurrentWeapon != null && player.isAlive ? 1.0f : 0.0f;

        if (CurrentWeapon != null)
        {
            //RIGHT HAND IK 
            anim.SetIKPositionWeight(AvatarIKGoal.RightHand, weight);
            anim.SetIKRotationWeight(AvatarIKGoal.RightHand, weight);
            anim.SetIKHintPositionWeight(AvatarIKHint.RightElbow, weight);

            anim.SetIKHintPosition(AvatarIKHint.RightElbow, CurrentWeapon.RightHandIKHint.position);
            anim.SetIKPosition(AvatarIKGoal.RightHand, CurrentWeapon.RightHandIK.position);
            anim.SetIKRotation(AvatarIKGoal.RightHand, CurrentWeapon.RightHandIK.rotation);

            //LEFT HAND IK 
            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, weight);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, weight);
            anim.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, weight);
            anim.SetIKHintPosition(AvatarIKHint.LeftElbow, CurrentWeapon.LeftHandIKHint.position);
            anim.SetIKPosition(AvatarIKGoal.LeftHand, CurrentWeapon.LeftHandIK.position);
            anim.SetIKRotation(AvatarIKGoal.LeftHand, CurrentWeapon.LeftHandIK.rotation);
        }
    }



    void OnFireInputRecieved(bool shouldFire)
    {
        if (!player.isAlive)
            return;
        if (player.isPaused)
            return;

        if (shouldFire)
        {
            CurrentWeapon?.PullTrigger();
        }
        else
        {
            var ngen = FindObjectOfType<NotificationGenerator>();

            if (CurrentWeapon.isOutOfAmmo)
            {
                ngen.GeneratePopup("Out of Ammo!!");
            }
            if (CurrentWeapon.isReloading)
            {
                ngen.GeneratePopup("Reloading!");
            }
        }
    }

    void OnAimInputRecieved(bool shouldAim)
    {
        if (!player.isAlive)
            return;
        if (player.isPaused)
            return;

        isAiming = shouldAim;
    }

    void OnReloadInputRecieved(bool shouldReload)
    {
        if (!player.isAlive)
            return;
        if (player.isPaused)
            return;

        if (shouldReload)
        {
            var ngen = FindObjectOfType<NotificationGenerator>();
            ngen.GeneratePopup("Reloading!");

            CurrentWeapon?.Reload();
        }
    }

    void UpdateState()
    {
        var isSprinting = locomotion.CurrentState == LocomotionState.Sprint;
        var isCrouching = locomotion.CurrentState == LocomotionState.Crouch;

        if (isSprinting)
        {
            state = EquippedWeaponState.Run;
        }
        else if (isCrouching)
        {
            state = EquippedWeaponState.Crouch;
        }
        else if (isAiming)
        {
            state = EquippedWeaponState.AimDownSight;
        }
        else if (!isAiming && !isSprinting)
        {
            state = EquippedWeaponState.Idle;
        }
    }

    void UpdateWeaponPosition()
    {
        switch (state)
        {
            case EquippedWeaponState.Idle:
                {
                    targetPosition = idlePose.position;
                }
                break;
            case EquippedWeaponState.Run:
                {
                    targetPosition = runPose.position;
                }
                break;
            case EquippedWeaponState.Crouch:
                {
                    targetPosition = crouchPose.position;
                }
                break;
            case EquippedWeaponState.AimDownSight:
                {
                    targetPosition = aimDownSightPose.position;
                }
                break;
        }
    }


    public int GetMagizineSize()
    {
        return CurrentWeapon.MaxMagazineCount;
    }

    public int GetStorageSize()
    {
        return CurrentWeapon.MaxStorageCount;
    }

    public bool AddAmmo(int amount)
    {
        return CurrentWeapon.AddAmmoToStorage(amount);
    }
}
