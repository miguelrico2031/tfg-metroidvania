using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/PlayerStats")]
public class PlayerStats : ScriptableObject, IAnimationStats, ICombatStats, IHealthStats, IPerceptionStats
{
    [field: Header("Movement"), SerializeField, Tooltip("Movement speed when grounded.")]
    public float GroundSpeed { get; private set; }

    [field: SerializeField, Tooltip("Movement speed when airborne.")]
    public float AirSpeed { get; private set; }
    [field: SerializeField, Tooltip("If disabled the player doesn't move when it changes direction.")]
    public bool MoveOnChangeDirection { get; private set; }


    [field: Header("Jump"), SerializeField, Tooltip("Jump force to achieve max jump height.")]
    public float JumpForce { get; private set; }

    [field: SerializeField, Range(0f, 1f), Tooltip("Value to scale the vertical velocity when releasing the jump button " +
        "before achieveing max jump height, to allow jumping lower than max jump height.")]
    public float JumpCutMultiplier { get; private set; }

    [field: SerializeField, Tooltip("Time window in seconds after pressing jump where the input is remembered " +
        "and executed upon landing.")]
    public float JumpBufferTime { get; private set; }

    [field: SerializeField, Tooltip("Time window in seconds after leaving the ground where jump is still allowed.")]
    public float CoyoteTime { get; private set; }


    [field: Header("Gravity"), SerializeField, Tooltip("Gravity scale when grounded or jumping upwards.")]
    public float RisingGravity { get; private set; }

    [field: SerializeField, Tooltip("Gravity scale when falling.")]
    public float FallingGravity { get; private set; }


    [field: Header("Dash"), SerializeField, Tooltip("Distance in game units the dash can traverse.")]
    public float DashDistance { get; private set; }

    [field: SerializeField, Tooltip("Time in seconds the dash will take.")]
    public float DashDuration { get; private set; }

    [field: SerializeField, Tooltip("Time window in seconds after pressing dash where the input is remembered " +
        "and executed upon landing.")]
    public float DashBufferTime { get; private set; }


    [field: Header("Ground Check"), SerializeField, Tooltip("Size of the box used to detect the ground at the player feet.")]
    public Vector2 GroundCheckSize { get; private set; }

    [field: SerializeField, Tooltip("How separated is the ground check box from the player feet.")]
    public float GroundCheckOffset { get; private set; }

    [field: SerializeField, Tooltip("Layers counting as ground for gorund check.")]
    public LayerMask GroundLayers { get; private set; }


    [field: Header("Obstacle Check"), SerializeField, Tooltip("Width of the box used to detect obstacles in front of the player (used for dashing).")]
    public Vector2 ObstacleCheckSize { get; private set; }

    [field: SerializeField, Tooltip("How separated is the obstacle check box from the player collider center left point.")]
    public float ObstacleCheckOffset { get; private set; }

    [field: SerializeField, Tooltip("Layers counting as obstacles for obstacle check.")]
    public LayerMask ObstacleLayers { get; private set; }


    [field: Header("Health & Damage"), SerializeField, Tooltip("Max amount of health the player can have.")]
    public int MaxHealth { get; private set; }

    [field: SerializeField, Tooltip("Max amount of heals the player can carry.")]
    public int MaxHeals { get; private set; }

    [field: SerializeField, Tooltip("Amount of health a heal heals.")]
    public int HealAmount { get; private set; }

    [field: SerializeField, Tooltip("Material of the damage flash effect.")]
    public Material DamageFlashMaterial { get; private set; }

    [field: SerializeField, Tooltip("Duration of the Damage Flash animation.")]
    public float DamageFlashAnimationDuration { get; private set; }


    [field: Header("Attack"), SerializeField, Tooltip("Time window in seconds after pressing attack where the input is remembered " +
        "when attacking is allowed")]
    public float AttackBufferTime { get; private set; }

    [field: SerializeField, Tooltip("Time window in seconds between completing the first attack and attacking again where that attack will be a " +
        "second attack combo instead of the first attack again.")]
    public float AdvanceAttackComboBufferTime { get; private set; }

    [field: SerializeField, Tooltip("Number of attacks in the attack combo.")]
    public int AttackComboCount { get; private set; }

    [field: SerializeField, Tooltip("Time window in seconds after being attacked when the player becomes invulnerable to other attacks.")]
    public float HitInvulnerabilityTime { get; private set; }


    //This is not currenlty used in the player components but it implements IPerceptionStats
    public Vector2 EdgeCheckOffset { get; }
    public float EdgeCheckDepth { get; }
}