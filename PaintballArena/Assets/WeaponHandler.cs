using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public abstract class WeaponHandler : MonoBehaviour
{

    [SerializeField] protected GameObject weaponPrefab;
    [SerializeField] protected GameObject projectilePrefab;

    [field: SerializeField, ReadOnly] public EquippedWeapon CurrentWeapon { get; set; }

    public List<WeaponStatePose> poses = new List<WeaponStatePose>();

    // cache
    protected Vector3 weaponPosition = Vector3.zero;
    
    public bool isAiming { get; protected set; }
    
    [field: SerializeField, ReadOnly] public EquippedWeaponState WeaponState { get; protected set; }
    [field: SerializeField] public Transform WeaponParent { get; set; }

    [Header("Attack Behavior Settings")]
    public float minAttackRange = 2.0f;
    public float maxAttackRange = 10.0f;

    public Action OnAmmoChanged;

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {
        if (CurrentWeapon == null)
            return;

        OnStateUpdate();
        UpdateWeaponPosition();
    }

    public abstract void OnStateUpdate();

    void UpdateWeaponPosition()
    {
        if (poses.Count <= 0)
            poses = CurrentWeapon.transform.parent.GetComponentsInChildren<WeaponStatePose>().ToList();


        var currentPos = poses.Find(pose => pose.State == WeaponState).transform;
        weaponPosition = currentPos.localPosition;
        CurrentWeapon.transform.localPosition = weaponPosition;
        //switch (WeaponState)
        //{
        //    case EquippedWeaponState.Idle:
        //        {
        //        }
        //        break;
        //    case EquippedWeaponState.Run:
        //        {
        //            var runPose = poses.Find(pose => pose.State == EquippedWeaponState.Run).transform;

        //            weaponPosition = runPose.position;
        //        }
        //        break;
        //    case EquippedWeaponState.Crouch:
        //        {
        //            var crouchPose = poses.Find(pose => pose.State == EquippedWeaponState.Crouch).transform;
        //            weaponPosition = crouchPose.position;
        //        }
        //        break;
        //    case EquippedWeaponState.AimDownSight:
        //        {
        //            var aimDownSightPose = poses.Find(pose => pose.State == EquippedWeaponState.AimDownSight).transform;
        //            weaponPosition = aimDownSightPose.position;
        //        }
        //        break;
        //}
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


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minAttackRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxAttackRange);
    }

    internal void DropWeapon()
    {
        Destroy(CurrentWeapon.gameObject);
    }
}
