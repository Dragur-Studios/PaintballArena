using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    public enum AmmoPickupType
    {
        Magazine,
        Crate
    };
    public AmmoPickupType type;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var weaponHandler = other.GetComponent<PlayerWeaponHandler>();
            int amountToAdd = 0;
            switch (type)
            {
                case AmmoPickupType.Magazine:
                    amountToAdd = weaponHandler.GetMagizineSize();
                    break;
                case AmmoPickupType.Crate:
                    amountToAdd = weaponHandler.GetStorageSize();
                    break;
            }

            if (weaponHandler.AddAmmo(amountToAdd))
            {
                Destroy(gameObject);
            }
           

        }
    }
}
