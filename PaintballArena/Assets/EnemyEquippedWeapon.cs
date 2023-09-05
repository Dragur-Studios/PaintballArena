//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Experimental.Rendering;




//public class EnemyEquippedWeapon : EquippedWeapon
//{
//    [Header("Handling Settings")]
//    [SerializeField] float fireRateRPM = 60.0f;

//    [SerializeField] float reloadTimeS = 1.35f;

//    [Header("Projectile Settings")]
//    [SerializeField] Transform muzzle;
//    [SerializeField] GameObject projectilePrefab;

//    [field: Header("IK Settings")]
//    [field: Header("Left Hand")]
    
//    bool canFire = true;
//    public bool isReloading = false;
//    public bool isOutOfAmmo { get => CurrentStorageCount <= 0 && CurrentMagazineCount <= 0; }

//    // a subscribable action to call consectutive functions (void()) 
//    Action OnTriggerPulled;

//    private void Start()
//    {
//        CurrentMagazineCount = MaxMagazineCount;
//        CurrentStorageCount = MaxStorageCount;

//        OnTriggerPulled += StartFiring;
//        OnTriggerPulled += SpawnProjectile;
//        OnTriggerPulled += RechamberRound;

//    }

//    public void Reload()
//    {
//        if (CurrentMagazineCount >= MaxMagazineCount || CurrentStorageCount <= 0)
//            return;

//        StartReload();
//    }

//    public void PullTrigger()
//    {
//        if (!canFire)
//        {
//            Debug.Log($"Enemy Cannot Fire for some reason... mag: {CurrentMagazineCount} storage: {CurrentStorageCount}");
//            return;
//        }

//        OnTriggerPulled?.Invoke();
//    }

//    void RechamberRound()
//    {
//        if (CurrentMagazineCount <= 0)
//        {
//            CurrentMagazineCount = 0;
//            StartReload();
//            return;

//        }

//        CurrentMagazineCount--;
   
//        Invoke(nameof(StopFiring), 60.0f / fireRateRPM);
//    }

//    void StartReload()
//    {
//        isReloading = true;
//        canFire = false;
//        if (CurrentStorageCount <= 0)
//        {
//            // there is nothing to reload. we cannot reload.
//            CurrentStorageCount = 0;
//            isReloading = false;
//            return;
//        }

//        // recycle ammo..
//        int currentMagCount = CurrentMagazineCount;
//        if (currentMagCount > 0)
//        {
//            AddAmmoToStorage(currentMagCount);
//        }

//        Invoke(nameof(StopReloading), reloadTimeS);
//    }

//    private void RemoveAmmoFromStorage(int amount)
//    {
//        // if there is nothing to remove from the sorage break early
//        if (CurrentStorageCount <= 0)
//            return;
//        // if we have some but not enough to fill the mag. add it
//        else if (amount > CurrentStorageCount && CurrentStorageCount > 0)
//        {
//            int remaining = CurrentStorageCount;
//            CurrentStorageCount = 0;              // just remove everything from the storage
//            CurrentMagazineCount = remaining;     // add what is in the storage
//        }
//        // there is sufficient ammo, remove a magazine
//        else
//        {
//            CurrentStorageCount -= amount;              // remove the amount requested
//            CurrentMagazineCount = amount;              // update the magazine
//        }
//    }

//    void StartFiring()
//    {
//        canFire = false;
//    }

//    void StopFiring()
//    {
//        canFire = true;
//    }
//    void StopReloading()
//    {
//        RemoveAmmoFromStorage(MaxMagazineCount);
     
//        isReloading = false;
//        canFire = true;
//    }

//    void SpawnProjectile()
//    {
//        var go = Instantiate(projectilePrefab);

//        go.transform.position = muzzle.position;
//        go.transform.rotation = muzzle.rotation;

//        var projectile = go.GetComponent<Projectile>();
//        projectile.Fire();

//    }

//    internal bool AddAmmoToStorage(int amount)
//    {
//        if (CurrentStorageCount >= MaxStorageCount)
//        {
//            CurrentStorageCount = MaxStorageCount; // just to be sure because of the above check could result in having too much ammo
//            return false;
//        }

//        // if the amount is equal to the max storage amount it means that we are adding from a crate.
//        // go ahead and skip this step if we are picking up a crate.
//        if (amount != MaxStorageCount)
//        {
//            var sum = (CurrentStorageCount + amount);
//            var diff = sum - MaxStorageCount;
//            if (diff >= 10) // tolerence of 10 being added before it cannot be picked up
//            {
//                return false;
//            }
//        }


//        bool shouldReload = false;

//        // where we out of ammo when we picked this up?
//        if (CurrentStorageCount == 0)
//            shouldReload = true; // trigger a reload automatically 

//        CurrentStorageCount += amount;
//        CurrentStorageCount = Mathf.Clamp(CurrentStorageCount, 0, MaxStorageCount);
  
//        if (shouldReload)
//        {
//            StartReload();
//        }

//        return true;

//    }
//}
