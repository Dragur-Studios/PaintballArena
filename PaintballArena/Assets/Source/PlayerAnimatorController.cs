using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour {

    
    Transform leftHand = null;
    Transform rightHand = null;

    Animator anim;

    PlayerWeaponHandler weaponHandler;


    private void Start()
    {
        anim = GetComponent<Animator>();
       
        weaponHandler = GetComponent<PlayerWeaponHandler>();
    }

    private void OnAnimatorMove()
    {
        
    }

    
}
