using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.PlasticSCM.Editor.WebApi;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerWeaponHandler : MonoBehaviour
{
    public Action OnAmmoChanged;

    [Header("Poses")]
    [SerializeField] Transform aimDownSightPose;
    [SerializeField] Transform idlePose;
    [SerializeField] Transform runPose;
    [SerializeField] Transform crouchPose;

    [Header("Cache")]
    [SerializeField, ReadOnly] PlayerWeaponState state;
    [SerializeField, ReadOnly] bool isAiming = false;
    
    [field: SerializeField] public PlayerWeapon CurrentWeapon { get; private set; }

    Animator anim;
    Vector3 targetPosition = Vector3.zero;


    PlayerLocomotion locomotion;
    PlayerInputDispatcher dispatcher;



    public enum PlayerWeaponState
    {
        Idle,
        Run,
        Crouch,
        AimDownSight,
    }

    [Header("Status Screens")]
    [SerializeField] TMP_Text magazineText;
    [SerializeField] TMP_Text ammoStorageText;
    [SerializeField] TMP_Text reloadingText;
    [SerializeField] TMP_Text outOfAmmoText;

  
    private void Start()
    {
        dispatcher = GetComponent<PlayerInputDispatcher>();
        dispatcher.OnFireInputHeld += OnFireInputRecieved;
        dispatcher.OnAimInputRecieved += OnAimInputRecieved;
        dispatcher.OnReloadInputRecieved += OnReloadInputRecieved;

        locomotion = GetComponent<PlayerLocomotion>();

        anim = GetComponent<Animator>();

        OnAmmoChanged += UpdateCurrentAmmoText;


    }

    void Update()
    {
        UpdateState();

        UpdateWeaponPosition();
    }
    void FixedUpdate()
    {
        var cur = CurrentWeapon.transform.position;
        CurrentWeapon.transform.position = Vector3.Lerp(cur, targetPosition, 10.0f * Time.deltaTime);
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


    void UpdateCurrentAmmoText()
    {
        var curAmmo = CurrentWeapon.CurrentMagazineCount;
        var curStorage = CurrentWeapon.CurrentStorageCount;

        magazineText.text = curAmmo.ToString();
        ammoStorageText.text = curStorage.ToString();

    }

    void OnFireInputRecieved(bool shouldFire)
    {
        if(shouldFire) CurrentWeapon?.PullTrigger();
    }

    void OnAimInputRecieved(bool shouldAim)
    {
        isAiming = shouldAim;
    }

    void OnReloadInputRecieved(bool shouldReload)
    {
        CurrentWeapon?.Reload();
    }

    void UpdateState()
    {
        var isSprinting = locomotion.CurrentState == PlayerLocomotion.LocomotionState.Sprint;
        var isCrouching = locomotion.CurrentState == PlayerLocomotion.LocomotionState.Crouch;

        var isReloading = CurrentWeapon.isReloading;
        var isOutOfAmmo = CurrentWeapon.isOutOfAmmo;

        reloadingText.enabled = isReloading;
        outOfAmmoText.enabled = isOutOfAmmo;

        if (isSprinting)
        {
            state = PlayerWeaponState.Run;
        }
        else if (isCrouching)
        {
            state = PlayerWeaponState.Crouch;
        }
        else if (isAiming)
        {
            state = PlayerWeaponState.AimDownSight;
        }
        else if (!isAiming && !isSprinting)
        {
            state = PlayerWeaponState.Idle;
        }
    }

    void UpdateWeaponPosition()
    {
        switch (state)
        {
            case PlayerWeaponState.Idle:
                {
                    targetPosition = idlePose.position;
                }
                break;
            case PlayerWeaponState.Run:
                {
                    targetPosition = runPose.position;
                }
                break;
            case PlayerWeaponState.Crouch:
                {
                    targetPosition = crouchPose.position;
                }
                break;
            case PlayerWeaponState.AimDownSight:
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
