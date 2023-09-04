using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AmmoUIHandler : MonoBehaviour
{
    PlayerWeaponHandler handler;

    [SerializeField] TMP_Text magazineText;
    [SerializeField] TMP_Text ammoStorageText;

    private void Start()
    {
        handler = FindObjectOfType<PlayerWeaponHandler>();
        handler.OnAmmoChanged += UpdateCurrentAmmoText;
    }

    void UpdateCurrentAmmoText()
    {
        var curAmmo = handler.CurrentWeapon.CurrentMagazineCount;
        var curStorage = handler.CurrentWeapon.CurrentStorageCount;

        magazineText.text = curAmmo.ToString();
        ammoStorageText.text = curStorage.ToString();

    }


}
