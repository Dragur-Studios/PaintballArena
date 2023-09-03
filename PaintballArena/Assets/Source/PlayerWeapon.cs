using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class PlayerWeapon : MonoBehaviour
{
    [Header("Handling Settings")]
    [SerializeField] float fireRateRPM = 60.0f;
    [field: SerializeField] public int MaxMagazineCount { get; private set; } = 30;
    [field: SerializeField] public int MaxStorageCount { get; private set; } = 300;
    [HideInInspector] public int CurrentMagazineCount { get; private set; }
    [HideInInspector] public int CurrentStorageCount { get; private set; }

    [SerializeField] float reloadTimeS = 1.35f;

    [Header("Projectile Settings")]
    [SerializeField] Transform muzzle;
    [SerializeField] GameObject projectilePrefab;

    [field: Header("IK Settings")]
    [field: Header("Left Hand")]
    [field: SerializeField ]public Transform LeftHandIK{ get; private set; }
    [field: SerializeField ]public Transform LeftHandIKHint{ get; private set; }
    [field: Header("Right Hand")]
    [field: SerializeField ]public Transform RightHandIK { get; private set; }
    [field: SerializeField ]public Transform RightHandIKHint { get; private set; }

    bool canFire = true;
    public bool isReloading = false;
    public bool isOutOfAmmo { get => CurrentStorageCount <= 0 && CurrentMagazineCount <= 0; }

    // a subscribable action to call consectutive functions (void()) 
    Action OnTriggerPulled;

    PlayerWeaponHandler handler;

    private void Start()
    {
        handler = FindObjectOfType<PlayerWeaponHandler>();

        CurrentMagazineCount = MaxMagazineCount;
        CurrentStorageCount = MaxStorageCount;

        handler.OnAmmoChanged?.Invoke();

        OnTriggerPulled += StartFiring;
        OnTriggerPulled += SpawnProjectile;
        OnTriggerPulled += RechamberRound;

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
        if(CurrentMagazineCount <= 0)
        {
            CurrentMagazineCount = 0;
            StartReload();
            return;

        }

        CurrentMagazineCount--;
        handler.OnAmmoChanged?.Invoke();

        Invoke(nameof(StopFiring), 60.0f / fireRateRPM );
    }

    void StartReload()
    {
        isReloading = true;

        if(CurrentStorageCount <= 0)
        {
            CurrentStorageCount = 0;
            isReloading = false;
            return;
        }

        CurrentStorageCount -= MaxMagazineCount;
        CurrentMagazineCount = MaxMagazineCount;
        handler.OnAmmoChanged?.Invoke();

        Invoke(nameof(StopReloading), reloadTimeS );
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
        isReloading = false;
        StopFiring();
    }
    
    void SpawnProjectile()
    {
        var go = Instantiate(projectilePrefab);

        go.transform.position = muzzle.position;
        go.transform.rotation = muzzle.rotation;

    }

    internal bool AddAmmoToStorage(int amount)
    {
        var sum = (CurrentStorageCount + amount);
        var diff = sum - MaxStorageCount;
        if (sum >= 10) // tolerence of 10 being added before it cannot be picked up
        {
            return false;
        }

        bool shouldReload = false;

        // where we out of ammo when we picked this up?
        if(CurrentStorageCount == 0)
            shouldReload = true; // trigger a reload automatically 

        CurrentStorageCount += amount;
        CurrentStorageCount = Mathf.Clamp(CurrentStorageCount, 0, MaxStorageCount);
        handler.OnAmmoChanged?.Invoke();

        if (shouldReload)
        {
            StartReload();
        }

        return true;

    }
}
