using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MomentumExtension : MonoBehaviour
{
    private PlayerMovement playerMovement;

    [Header("Increase/Decrease")]
    
    [Range(0f,10f)] public float momentumIncreaseFactor = 1;
    [Range(0f, 10f)] public float momentumDecreaseFactorOnGround = 2;
    [Range(0f, 10f)] public float momentumDecreaseFactorInAir = 0.5f;

    [Header("Boundaries")]
    
    [Range(0f, 10f)] public float minimalMomentum = 3f;

    [Header("State Settings")]
    
    public List<MovementState> movementStates = new List<MovementState>()
    {
        new MovementState("Walking", PlayerMovement.MovementMode.walking, 1, 1),
        new MovementState("Sprinting",PlayerMovement.MovementMode.sprinting, 1, 1),
        new MovementState("Crouching", PlayerMovement.MovementMode.crouching, 1, 1),
        new MovementState("Swinging", PlayerMovement.MovementMode.swinging, 1, 1),
        new MovementState("Sliding", PlayerMovement.MovementMode.sliding, 2, 1),
        new MovementState("Wallrunning", PlayerMovement.MovementMode.wallrunning, 1, 1),
        new MovementState("Walljumping", PlayerMovement.MovementMode.walljumping, 1, 1),
        new MovementState("Climbing", PlayerMovement.MovementMode.climbing, 1, 1),
        new MovementState("Dashing", PlayerMovement.MovementMode.dashing, -1, 10),
    };

    public List<MovementState> hardcodedMovementStates = new List<MovementState>()
    {
        new MovementState("Unlimited", PlayerMovement.MovementMode.unlimited, -1, -1),
        new MovementState("Limited", PlayerMovement.MovementMode.limited, -1, 1),
        new MovementState("Freeze", PlayerMovement.MovementMode.freeze, -1, -1),
    };


    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    public MovementState GetMovementState(PlayerMovement.MovementMode movementMode)
    {
        foreach (MovementState state in movementStates)
        {
            if (state.movementMode == movementMode)
            {
                return state;
            }
        }
        
        foreach (MovementState state in hardcodedMovementStates)
        {
            if (state.movementMode == movementMode)
            {
                return state;
            }
        }

        return null;
    }

    public float GetIncreaseSpeedChangeFactor(PlayerMovement.MovementMode movementMode)
    {
        float speedChangeFactor = 0f;
        
        MovementState movementState = GetMovementState(movementMode);
        
        if (movementState != null)
        {
            if (movementState.speedBuildupFactor == -1)
            {
                speedChangeFactor = -1;
            }
                
            else
            {
                speedChangeFactor = momentumIncreaseFactor * movementState.speedBuildupFactor;
            }
        }
        else
        {
            Debug.LogError("MovementState not found");
            
            speedChangeFactor = -1;
        }

        return speedChangeFactor;
    }

    public float GetDecreaseSpeedChangeFactor(PlayerMovement.MovementMode movementMode)
    {
        float speedChangeFactor = 0f;

        MovementState movementState = GetMovementState(movementMode);
        
        if (movementState != null)
        {
            if (movementState.speedBuilddownFactor == -1)
            {
                speedChangeFactor = -1;
            }
                
            else
            {
                speedChangeFactor = momentumIncreaseFactor * movementState.speedBuilddownFactor;
            }
        }
        else
        {
            speedChangeFactor = -1;
        }

        return speedChangeFactor;
    }

    public float GetSurfaceSpeedDecreaseFactor(bool grounded)
    {
        return grounded ? momentumDecreaseFactorOnGround : momentumDecreaseFactorInAir;
    }

    public bool IsStateAllowed(PlayerMovement.MovementMode movementMode, float currMomentum)
    {
        MovementState movementState = GetMovementState(movementMode);
        bool stateAllowed = true;

        if(currMomentum < movementState.minNeededMomentum)
            stateAllowed = false;
        else if (currMomentum > movementState.maxAllowedMomentum)
            stateAllowed = false;

        // Debug.Log($"CurrMomentum {currMomentum}, MinMomentum {movementState.minNeededMomentum},
        // MaxMomentum {movementState.maxAllowedMomentum} -> State {movementState.stateName} allowed -> {stateAllowed}");

        return stateAllowed;
    }
}

[Serializable]
public class MovementState
{
    public string stateName;

    public PlayerMovement.MovementMode movementMode;

    [Tooltip("-1 means instant speed change")]
    public float speedBuildupFactor = -1;
    [Tooltip("-1 means instant speed change")]
    public float speedBuilddownFactor = -1;

    [Range(0f, 100f)]
    public float minNeededMomentum = 0f;
    [Range(0f, 100f)]
    public float maxAllowedMomentum = 100f;

    public MovementState(string stateName, PlayerMovement.MovementMode movementMode, float speedBuildupFactor, float speedBuilddownFactor)
    {
        this.stateName = stateName;
        this.movementMode = movementMode;
        this.speedBuildupFactor = speedBuildupFactor;
        this.speedBuilddownFactor = speedBuilddownFactor;
    }

    public MovementState(PlayerMovement.MovementMode movementMode)
    {
        this.movementMode = movementMode;
    }
}