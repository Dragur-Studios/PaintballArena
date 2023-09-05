using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputDispatcher : MonoBehaviour
{
    PlayerInputReciever reciever;

    public Action<Vector2> OnMoveInputRecieved;
    public Action<Vector2> OnLookInputRecieved;
    
    public Action<bool> OnAimInputRecieved;

    public Action<bool> OnFireInputDown;
    public Action<bool> OnFireInputHeld;
    public Action<bool> OnFireInputReleased;
    
    public Action<bool> OnJumpInputRecieved;
    public Action<bool> OnReloadInputRecieved;
    public Action<bool> OnCrouchInputRecieved;
    public Action<bool> OnSprintInputRecieved;

    bool isPaused = false;

    float FireInput = 0;

    private void OnEnable()
    {
        if(reciever == null)
        {
            reciever = new PlayerInputReciever();

            // aquire and dispatch actions or values

            // player -defaults
            reciever.Player.Move.performed += DispatchMovementInput;
            reciever.Player.Look.performed += DispatchLookInput;
            // player -combat
            reciever.Combat.Aim.performed += DispatchAimInput;
            reciever.Combat.Fire.performed += AquireFireInput;
            reciever.Combat.Jump.performed += DispatchJumpInput;
            reciever.Combat.Reload.performed += DispatchReloadInput;
            reciever.Combat.Crouch.performed += DispatchCrouchInput;

            reciever.Game.Pause.performed += DispatchPauseInput;

            reciever.Enable();
        }
    }

    private void Update()
    {
        DispatchFireInput(FireInput != 0);
    }

    private void OnDisable()
    {
        if (reciever != null)
        {

            // player -defaults
            reciever.Player.Move.performed -= DispatchMovementInput;
            reciever.Player.Look.performed -= DispatchLookInput;
            reciever.Player.Sprint.performed -= DispatchSprintInput;

            // player -combat
            reciever.Combat.Aim.performed -= DispatchAimInput;
            reciever.Combat.Fire.performed -= AquireFireInput;

            reciever.Combat.Jump.performed -= DispatchJumpInput;
            reciever.Combat.Reload.performed -= DispatchReloadInput;
            reciever.Combat.Crouch.performed -= DispatchCrouchInput;

            reciever.Disable();
        }
    }

    void DispatchMovementInput(InputAction.CallbackContext ctx) => OnMoveInputRecieved?.Invoke(ctx.ReadValue<Vector2>());
    void DispatchLookInput(InputAction.CallbackContext ctx) => OnLookInputRecieved?.Invoke(ctx.ReadValue<Vector2>());
    void DispatchSprintInput(InputAction.CallbackContext ctx)=> OnSprintInputRecieved?.Invoke(ctx.ReadValue<float>() != 0);
    void DispatchPauseInput(InputAction.CallbackContext ctx) => OnPauseButtonRecieved(ctx.ReadValue<float>() != 0);

    void OnPauseButtonRecieved(bool shouldPause)
    {
        if (shouldPause && !isPaused)
        {
            isPaused = true;


            GameManager.SignalGamePaused();
        }
        else if (shouldPause && isPaused)
        {
            isPaused = false;

            GameManager.SignalGameUnaused();
        }
    }


    void DispatchAimInput(InputAction.CallbackContext ctx)=> OnAimInputRecieved?.Invoke(ctx.ReadValue<float>() != 0);

    bool hasPressedFireThisFrame = false;

    void AquireFireInput(InputAction.CallbackContext ctx)
    {
        FireInput = ctx.ReadValue<float>();
    }

    void DispatchFireInput(bool fire) {

        // if we havent press the fire button this frame but we DID just press it
        if (!hasPressedFireThisFrame && fire == true)
        {
            OnFireInputDown?.Invoke(fire);  // fire down input
            hasPressedFireThisFrame = true; // close gate until the button is released
        }

        // poll the fire input held each frame
        if(fire == true)
        {
            OnFireInputHeld?.Invoke(fire);
        }

        // if we have been pressing the input and have just released it.
        if (hasPressedFireThisFrame && fire == false)
        {
            OnFireInputReleased?.Invoke(fire);
            hasPressedFireThisFrame = false;
        }



    }
    void DispatchJumpInput(InputAction.CallbackContext ctx)=> OnJumpInputRecieved?.Invoke(ctx.ReadValue<float>() != 0);
    void DispatchReloadInput(InputAction.CallbackContext ctx) => OnReloadInputRecieved?.Invoke(ctx.ReadValue<float>() != 0);
    void DispatchCrouchInput(InputAction.CallbackContext ctx) => OnCrouchInputRecieved?.Invoke(ctx.ReadValue<float>() != 0);


}
