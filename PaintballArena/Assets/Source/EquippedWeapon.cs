using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class EquippedWeapon : MonoBehaviour
{
    [SerializeField] WeaponData startData = null; 
    WeaponData data; 

    [Header("Projectile Settings")]
    [SerializeField] Transform muzzle;
    GameObject projectilePrefab;

    public Transform LeftHandIK{ get; private set; }
    public Transform LeftHandIKHint{ get; private set; }
    public Transform RightHandIK { get; private set; }
    public Transform RightHandIKHint { get; private set; }

    bool canFire = true;
    public bool isOutOfAmmo { get => data.CurrentStorageCount <= 0 && data.CurrentMagazineCount <= 0; }

    [ReadOnly] public bool isReloading = false;
    // a subscribable action to call consectutive functions (void()) 
    Action OnTriggerPulled;

    WeaponHandler handler;

    public int MaxMagazineCount { get => data.MaxMagazineCount; }
    public int MaxStorageCount { get => data.MaxStorageCount; }

    public int CurrentMagazineCount { get => data.CurrentMagazineCount; }
    public int CurrentStorageCount { get => data.CurrentStorageCount;  }

    void Init(WeaponHandler weaponHandler)
    {
        // search for ikgoals

        var goals = GetComponentsInChildren<IKGoal>().ToList();
        var hints = transform.parent.GetComponentsInChildren<IKHint>().ToList();

        LeftHandIK = goals.First(g => g.GoalType == IKGoalType.LeftHand).transform;
        RightHandIK = goals.First(g => g.GoalType == IKGoalType.LeftHand).transform;
        RightHandIKHint = hints.First(h => h.HintType == IKHintType.RightElbow).transform;
        LeftHandIKHint = hints.First(h => h.HintType == IKHintType.LeftElbow).transform;


        // ensure the data buffer is not modifying the original
        data = Instantiate(startData); 

        handler = weaponHandler;
        handler = FindObjectOfType<PlayerWeaponHandler>();

        data.CurrentMagazineCount = data.MaxMagazineCount;
        data.CurrentStorageCount = data.MaxStorageCount;

        handler.OnAmmoChanged?.Invoke();

        OnTriggerPulled += StartFiring;
        OnTriggerPulled += SpawnProjectile;
        OnTriggerPulled += RechamberRound;
    }

  
    public void Reload()
    {
        if (data.CurrentMagazineCount >= data.MaxMagazineCount || data.CurrentStorageCount <= 0)
            return;

        StartReload();
    }

    public void PullTrigger()
    {
        if (!canFire)
        {
            return;
        }

        OnTriggerPulled?.Invoke();
    }

    void RechamberRound()
    {
        if(data.CurrentMagazineCount <= 0)
        {
            data.CurrentMagazineCount = 0;
            StartReload();
            return;

        }

        data.CurrentMagazineCount--;
        handler.OnAmmoChanged?.Invoke();

        Invoke(nameof(StopFiring), 60.0f / data.FireRate );
    }

    void StartReload()
    {
        isReloading = true;
        canFire = false;
        if (data.CurrentStorageCount <= 0)
        {
            // there is nothing to reload. we cannot reload.
            data.CurrentStorageCount = 0;
            isReloading = false;
            return;
        }

        // recycle ammo..
        int currentMagCount = data.CurrentMagazineCount;
        if (currentMagCount > 0)
        {
            AddAmmoToStorage(currentMagCount);
        }
        handler.OnAmmoChanged?.Invoke();


        Invoke(nameof(StopReloading), data.reloadTimeS);
    }

    private void RemoveAmmoFromStorage(int amount)
    {
        // if there is nothing to remove from the sorage break early
        if (data.CurrentStorageCount <= 0)
            return;
        // if we have some but not enough to fill the mag. add it
        else if ( amount > data.CurrentStorageCount && data.CurrentStorageCount > 0)
        {
            int remaining = data.CurrentStorageCount;
            data.CurrentStorageCount = 0;              // just remove everything from the storage
            data.CurrentMagazineCount = remaining;     // add what is in the storage
        }
        // there is sufficient ammo, remove a magazine
        else
        {
            data.CurrentStorageCount -= amount;              // remove the amount requested
            data.CurrentMagazineCount = amount;              // update the magazine
        }
    }

    void StartFiring()
    {
        canFire = false;
    }

    void StopFiring()
    {
        canFire = true;
    }
    void StopReloading()
    {
        RemoveAmmoFromStorage(data.MaxMagazineCount);
        handler.OnAmmoChanged?.Invoke();

        isReloading = false;
        canFire = true;
    }
    
    void SpawnProjectile()
    {
        var go = Instantiate(projectilePrefab);

        go.transform.position = muzzle.position;
        go.transform.rotation = muzzle.rotation;

        var projectile = go.GetComponent<Projectile>();
        projectile.Fire();

    }

    internal bool AddAmmoToStorage(int amount)
    {
        if (data.CurrentStorageCount >= data.MaxStorageCount)
        {
            data.CurrentStorageCount = data.MaxStorageCount; // just to be sure because of the above check could result in having too much ammo
            return false;
        }

        // if the amount is equal to the max storage amount it means that we are adding from a crate.
        // go ahead and skip this step if we are picking up a crate.
        if(amount != data.MaxStorageCount)
        {
            var sum = (data.CurrentStorageCount + amount);
            var diff = sum - data.MaxStorageCount;
            if (diff >= 10) // tolerence of 10 being added before it cannot be picked up
            {
                return false;
            }
        }
        

        bool shouldReload = false;

        // where we out of ammo when we picked this up?
        if(data.CurrentStorageCount == 0)
            shouldReload = true; // trigger a reload automatically 

        data.CurrentStorageCount += amount;
        data.CurrentStorageCount = Mathf.Clamp(data.CurrentStorageCount, 0, data.MaxStorageCount);
        handler.OnAmmoChanged?.Invoke();

        if (shouldReload)
        {
            StartReload();
        }

        return true;

    }

    public static void CreateWeaponAndGiveToHandler(WeaponHandler handler, GameObject weaponPrefab, GameObject projectilePrefab)
    {
        EquippedWeapon weapon = CreateWeaponForHandler(handler, weaponPrefab, projectilePrefab);
        GiveHandlerWeapon(handler, weapon);
    }

    static EquippedWeapon CreateWeaponForHandler(WeaponHandler handler, GameObject weaponPrefab, GameObject projectilePrefab)
    {
        var parent = handler.WeaponParent;
        var go = Instantiate(weaponPrefab);
        go.transform.SetParent(parent.transform, false);
        var weapon = go.GetComponent<EquippedWeapon>();
        weapon.projectilePrefab = projectilePrefab;
        return weapon;
    }

    public static void GiveHandlerWeapon(WeaponHandler target, EquippedWeapon weapon)
    {
        target.CurrentWeapon = weapon;
        weapon.Init(target);

    }
    public static void RemoveFrom(WeaponHandler target)
    {
        target.DropWeapon();
    }

}
