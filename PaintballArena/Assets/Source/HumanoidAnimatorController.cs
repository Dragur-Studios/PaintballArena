using System;
using UnityEngine;

/// <summary>
/// a animation controller for humanoid agents.
/// place next to the animator component and fire away. 
/// just a wrapper class for the animator class. but includes a vital override required for 
/// animation rigging to work properly with the rigidbody physics system.
/// </summary>


public class HumanoidAnimatorController : MonoBehaviour 
{    
    Animator anim;
    PlayerWeaponHandler weaponHandler;


    private void Start()
    {
        anim = GetComponent<Animator>();  
    }

    // DO NOT REMOVE!
    // required for rigidbody to move because of animation rigging packages
    private void OnAnimatorMove()
    {
        // repeast: DO NOT REMOVE
    }

    public void Crossfade(string id)
    {
        anim.CrossFade(id, 0.1f);
    }

    public void SetFloat(string id, float value)
    {
        anim.SetFloat(id, value);

    }

    public void SetFloatS(string id, float value, float t)
    {
        var cur = anim.GetFloat(id);
        var interpolated = Mathf.Lerp(cur, value, t);
        anim.SetFloat(id, interpolated);

    }

    public void SetBool(string id, bool value)
    {
        anim.SetBool(id, value);
    }

    public void Trigger(string id, bool reset=false)
    {
        if (reset)
        {
            anim.ResetTrigger(id);
        }

        anim.SetTrigger(id);
    }

    public void SetIKPositionWeight(AvatarIKGoal goal, float weight)
    {
        anim.SetIKPositionWeight(goal, weight);
    }

    public void SetIKRotationWeight(AvatarIKGoal goal, float weight)
    {
        anim.SetIKRotationWeight(goal, weight);
    }

    public void SetIKHintPositionWeight(AvatarIKHint hint, float weight)
    {
        anim.SetIKHintPositionWeight(hint, weight);
    }

    public void SetIKHintPosition(AvatarIKHint hint, Vector3 position)
    {
        anim.SetIKHintPosition(hint, position);
    }

    public void SetIKPosition(AvatarIKGoal goal, Vector3 position)
    {
        anim.SetIKPosition(goal, position);
    }

    public void SetIKRotation(AvatarIKGoal goal, Quaternion rotation)
    {
        anim.SetIKRotation(goal, rotation);
    }
}
