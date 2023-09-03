using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.PlasticSCM.Editor.WebApi;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerWeaponHandler : MonoBehaviour
{
    [field: SerializeField] public PlayerWeapon CurrentWeapon { get; private set; }

    State state;
    Vector3 targetPosition = Vector3.zero;

    [SerializeField] Transform aimDownSightPose;
    [SerializeField] Transform idlePose;
    [SerializeField] Transform runPose;
    [SerializeField] Transform crouchPose;

    bool isAiming = false;

    Animator anim;

    public Action OnAmmoChanged;

    public enum State
    {
        Idle,
        Run,
        Crouch,
        AimDownSight,
    }

    [SerializeField] TMP_Text magazineText;
    [SerializeField] TMP_Text ammoStorageText;
    [SerializeField] TMP_Text reloadingText;
    [SerializeField] TMP_Text outOfAmmoText;

    private void Start()
    {
        anim = GetComponent<Animator>();

        OnAmmoChanged += UpdateCurrentAmmoText;
        

    }

    void UpdateCurrentAmmoText()
    {
        var curAmmo = CurrentWeapon.CurrentMagazineCount;
        var curStorage = CurrentWeapon.CurrentStorageCount;

        magazineText.text = curAmmo.ToString();
        ammoStorageText.text = curStorage.ToString();

    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            CurrentWeapon?.PullTrigger();
        }

        isAiming = Input.GetKey(KeyCode.Mouse1);

        var isRunning = Input.GetKey(KeyCode.LeftShift);
        var isCrouching = Input.GetKey(KeyCode.C);

        var isReloading = CurrentWeapon.isReloading;
        var isOutOfAmmo = CurrentWeapon.isOutOfAmmo;

        reloadingText.enabled = isReloading;
        outOfAmmoText.enabled = isOutOfAmmo;

        if (isRunning)
        {
            state = State.Run;
        }
        if (isCrouching)
        {
            state = State.Crouch;
        }
        if(isAiming)
        {
            state = State.AimDownSight;
        }
        if (!isAiming && !isRunning)
        {
            state = State.Idle;
        }
        

        UpdateState();
    }

    private void FixedUpdate()
    {
        var cur = CurrentWeapon.transform.position;
        CurrentWeapon.transform.position = Vector3.Lerp(cur, targetPosition, 10.0f * Time.deltaTime);
    }

    private void UpdateState()
    {
        switch (state)
        {
            case State.Idle:
                {
                    targetPosition = idlePose.position;
                }
                break;
            case State.Run:
                {
                    targetPosition = runPose.position;
                }
                break;
            case State.Crouch:
                {
                    targetPosition = crouchPose.position;
                }
                break;
            case State.AimDownSight:
                {
                    targetPosition = aimDownSightPose.position;
                }
                break;
        }
    }

    public void OnAnimatorIK(int layerIndex)
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

    public int GetMagizineSize()
    {
        return CurrentWeapon.MaxMagazineCount;
    }

    public int GetStorageSize()
    {
        return CurrentWeapon.MaxStorageCount;
    }

    internal bool AddAmmo(int amount)
    {
        return CurrentWeapon.AddAmmoToStorage(amount);
    }
}
