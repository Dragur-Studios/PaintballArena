using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AmmoPickup;

public class AmmoSpawnPoint : MonoBehaviour
{
    [SerializeField] AmmoPickup ammoPrefab;
    AmmoPickup ammoPickup; // reference to the object. triggers respawn 

    [SerializeField] float pickupTimeDelay = 2.0f;

    void Start()
    {
        Respawn();    
    }

    void Respawn()
    {
        ammoPickup = Instantiate(ammoPrefab, transform, false);
        ammoPickup.transform.localPosition = Vector3.zero;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TryPickup(other.GetComponent<PlayerWeaponHandler>());
        }
    }



    void TryPickup(PlayerWeaponHandler weaponHandler)
    {
        if (weaponHandler == null)
            return;

        int amountToAdd = 0;
        switch (ammoPickup.Type)
        {
            case AmmoPickupType.Magazine:
                amountToAdd = weaponHandler.GetMagizineSize();
                break;
            case AmmoPickupType.Crate:
                amountToAdd = weaponHandler.GetStorageSize();
                break;
        }

        bool destroy = weaponHandler.AddAmmo(amountToAdd);

        if (destroy)
        {
            DestroyAmmoPickup();

            Invoke(nameof(Respawn), pickupTimeDelay);
        }

    }

    private void DestroyAmmoPickup()
    {
        Destroy(ammoPickup.gameObject);
        ammoPickup = null;
    }

   
}
