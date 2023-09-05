using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AmmoUIHandler : iGameStateListener
{
    PlayerWeaponHandler handler;

    [SerializeField] TMP_Text magazineText;
    [SerializeField] TMP_Text ammoStorageText;

   

    protected override void Start()
    {
        base.Start();

    }

    void UpdateCurrentAmmoText()
    {
        var curAmmo = handler.CurrentWeapon.CurrentMagazineCount;
        var curStorage = handler.CurrentWeapon.CurrentStorageCount;

        magazineText.text = curAmmo.ToString();
        ammoStorageText.text = curStorage.ToString();

    }

    public override void HandleGameOver()
    {
    }

    public override void HandleGamePaused()
    {
    }

    public override void HandleGameUnpaused()
    {
    }

    public override void HangleGameStarted()
    {
        handler = FindObjectOfType<PlayerWeaponHandler>();
        handler.OnAmmoChanged += UpdateCurrentAmmoText;
    }
}
