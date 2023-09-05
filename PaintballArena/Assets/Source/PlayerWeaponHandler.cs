using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;

public enum EquippedWeaponState
{
    Idle,
    Run,
    Crouch,
    AimDownSight,
}

public class PlayerWeaponHandler : WeaponHandler
{
    Animator anim;
    Vector3 targetPosition = Vector3.zero;

    Player player;
    PlayerLocomotion locomotion;
    PlayerInputDispatcher dispatcher;

    protected override void Start()
    {
        base.Start();

        dispatcher = GetComponent<PlayerInputDispatcher>();
        dispatcher.OnFireInputHeld += OnFireInputRecieved;
        dispatcher.OnAimInputRecieved += OnAimInputRecieved;
        dispatcher.OnReloadInputRecieved += OnReloadInputRecieved;

        player = GetComponent<Player>();
        locomotion = GetComponent<PlayerLocomotion>();

        anim = GetComponent<Animator>();


        EquippedWeapon.CreateWeaponAndGiveToHandler(this, weaponPrefab, projectilePrefab);

    }

    protected override void Update()
    {
        if (!player.isAlive)
            return;
        if (player.isPaused)
            return;
        if (!player.isGameStarted)
            return;

        base.Update();
    
    }
    void FixedUpdate()
    {
        if (!player.isAlive)
            return;
        if (player.isPaused)
            return;
        if(!player.isGameStarted) 
            return;

        //var cur = CurrentWeapon.transform.position;
        //CurrentWeapon.transform.position = targetPosition;
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

    public override void OnStateUpdate()
    {
        var isSprinting = locomotion.CurrentState == LocomotionState.Sprint;
        var isCrouching = locomotion.CurrentState == LocomotionState.Crouch;

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

}
