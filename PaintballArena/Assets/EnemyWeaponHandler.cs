using System;
using UnityEditor;
using static UnityEngine.EventSystems.EventTrigger;
using UnityEngine;

public class EnemyWeaponHandler : WeaponHandler
{
    EnemyLocomotion locomotion;
    

    protected override void Start()
    {
        base.Start();

        locomotion = GetComponent<EnemyLocomotion>();

        EquippedWeapon.CreateWeaponAndGiveToHandler(this, weaponPrefab, projectilePrefab);
        
    }
 
    public override void OnStateUpdate()
    {
        var isSprinting = locomotion.CurrentState == LocomotionState.Sprint;
        var isCrouching = locomotion.CurrentState == LocomotionState.Crouch;

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


    internal void AimAndFire()
    {
        var dir = (GetComponent<Enemy>().Target.position - transform.position);
        dir.y = 0;
        dir.Normalize();

        var lookdir = Quaternion.LookRotation(dir, transform.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookdir, 2.0f * Time.deltaTime);

        CurrentWeapon?.PullTrigger();
    }
}
