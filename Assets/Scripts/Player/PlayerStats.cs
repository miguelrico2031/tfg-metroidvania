using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    [field: Header("Movement"), SerializeField, Tooltip("Movement speed when grounded.")]
    public float GroundSpeed { get; private set; }
   
    [field: SerializeField, Tooltip("Movement speed when airborne.")]
    public float AirSpeed {get; private set; }


    [field: Header("Jump"), SerializeField, Tooltip("Jump force to achieve max jump height.")]
    public float JumpForce {get; private set; }
    
    [field: SerializeField, Range(0f, 1f),Tooltip("Value to scale the vertical velocity when releasing the jump button " +
        "before achieveing max jump height, to allow jumping lower than max jump height.")]
    public float JumpCutMultiplier {get; private set; }

    [field: SerializeField, Tooltip("Time window in seconds after pressing jump where the input is remembered " +
        "and executed upon landing.")]
    public float JumpBufferTime {get; private set; }

    [field: SerializeField, Tooltip("Time window in seconds after leaving the ground where jump is still allowed.")]
    public float CoyoteTime { get; private set; }


    [field: Header("Gravity"), SerializeField, Tooltip("Gravity scale when grounded or jumping upwards.")]
    public float RisingGravity {get; private set; }

    [field: SerializeField, Tooltip("Gravity scale when falling.")]
    public float FallingGravity {get; private set; }


    [field: Header("Ground Check"), SerializeField, Tooltip("Size of the box used to detect the ground at the player feet.")]
    public Vector2 GroundCheckSize {get; private set; }

    [field: SerializeField, Tooltip("How separated is the ground check box from the player feet.")]
    public float GroundCheckOffset { get; private set; }

    [field: SerializeField, Tooltip("Layers counting as ground for gorund check.")]
    public LayerMask GroundLayer {get; private set; }


    [field: Header("Stamina"), SerializeField, Tooltip("Max amount of stamina the player can have.")]
    public int MaxStamina {get; private set; }

    [field: SerializeField, Tooltip("Stamina points recovered per second when player stamina is below max.")]
    public int StaminaRecoveryRate {get; private set; }

    [SerializeField, Tooltip("How many stamina points each action consumes.")]
    private StaminaActionCostEntry[] m_StaminaActionCostsEntries;
    public IReadOnlyCollection<StaminaActionCostEntry> StaminaActionCosts => m_StaminaActionCostsEntries;
}

public enum PlayerStaminaAction
{
    Jump,
    Dash,
    Attack
}

[Serializable]
public struct StaminaActionCostEntry
{
    public PlayerStaminaAction Action;
    public int Cost;
}