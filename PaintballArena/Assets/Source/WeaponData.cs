using UnityEngine;

[CreateAssetMenu(menuName ="Create/Weapon Data", fileName ="(weapon_data)")]
public class WeaponData : ScriptableObject
{
    [Header("Handling Settings")]
    public float FireRate = 60.0f;
    [field: SerializeField] public int MaxMagazineCount { get; private set; } = 30;
    [field: SerializeField] public int MaxStorageCount { get; private set; } = 300;
    [field: SerializeField, ReadOnly] public int CurrentMagazineCount { get; set; }
    [field: SerializeField, ReadOnly] public int CurrentStorageCount { get; set; }

    public float reloadTimeS = 1.35f;
}
