using UnityEngine;


[CreateAssetMenu(menuName = "ScriptableObjects/EnemyStats")]
public class EnemyStats : ScriptableObject, IAnimationStats, ICombatStats, IHealthStats, IPerceptionStats
{
    [field: SerializeField] public int MaxHealth { get; private set; }

    [field: SerializeField] public Vector2 EdgeCheckOffset {get; private set; }

    [field: SerializeField] public float EdgeCheckDepth {get; private set; }

    [field: SerializeField] public Vector2 GroundCheckSize {get; private set; }

    [field: SerializeField] public float GroundCheckOffset {get; private set; }

    [field: SerializeField] public LayerMask GroundLayers {get; private set; }

    [field: SerializeField] public float CoyoteTime {get; private set; }

    [field: SerializeField] public Vector2 ObstacleCheckSize {get; private set; }

    [field: SerializeField] public float ObstacleCheckOffset {get; private set; }

    [field: SerializeField] public LayerMask ObstacleLayers {get; private set; }


    [field: SerializeField] public float DamageFlashAnimationDuration { get; private set; }


    [field: SerializeField] public float HitInvulnerabilityTime { get; private set; }

    [field: SerializeField] public float DelayBeforeAttacking { get; private set; }
    [field: SerializeField] public float DelayAfterAttacking { get; private set; }

    [field: SerializeField] public float DelayBeforeSeekingPost { get; private set; }

    public float AdvanceAttackComboBufferTime => 0f;
    public int AttackComboCount => 0;
}