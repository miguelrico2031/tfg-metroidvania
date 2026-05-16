using System;
using UnityEngine;
using UnityEngine.Assertions;

public class AttackTargetComponent : MonoBehaviour, IAttackTarget
{
    public Faction Faction => m_Faction == null ? Faction.MAX : m_Faction.Faction;

    public ResolvedAttack ResolvedAttackThisFrame { get; set; }
    public event Action OnAttackReceived;
    public bool IsInvulnerable => m_InvulnerabilitySources > 0;
    public bool IsAlive => m_Health is { CurrentHealth: > 0 };
    public int BlockingDirection { get; private set; } = 0;
    public bool IsBlocking => BlockingDirection != 0;

    [SerializeField] private DataReference<ICombatStats> m_Stats;

    private int m_InvulnerabilitySources = 0;
    private float m_HitInvulnerabilityTimer = 0f;
    private Hurtbox[] m_Hurtboxes;
    private HealthComponent m_Health;
    private FactionComponent m_Faction;

    public void AddInvulnerability()
    {
        if(++m_InvulnerabilitySources == 1)
        {
            foreach (var hurtbox in m_Hurtboxes)
            {
                hurtbox.enabled = false;
            }
        }
    }

    public void RemoveInvulnerability()
    {
        Assert.IsTrue(IsInvulnerable, "Tried to clear invulnerability without having set it.");
        if(--m_InvulnerabilitySources == 0)
        {
            foreach (var hurtbox in m_Hurtboxes)
            {
                hurtbox.enabled = true;
            }
        }
    }

    public void StartBlocking(bool right) => BlockingDirection = right ? 1 : -1;
    public void StopBlocking() => BlockingDirection = 0;

    public AttackResult ResolveAttack(AttackData attack)
    {
        if (ResolvedAttackThisFrame is not null || IsInvulnerable || !IsAlive)
            return AttackResult.Missed;

        if(IsBlockingAttack(attack))
        {
            ResolvedAttackThisFrame = new(attack.Damage, attack.BlockedKnockback, attack.Position, attack.Velocity, AttackResult.Blocked);
        }
        else
        {
            ResolvedAttackThisFrame = new(attack.Damage, attack.HitKnockback, attack.Position, attack.Velocity, AttackResult.Hit);
            if (m_Health != null)
            {
                m_Health.TakeDamage(attack.Damage);
            }
            if (IsAlive && m_Stats.Value.HitInvulnerabilityTime > 0.01f)
            {
                m_HitInvulnerabilityTimer = m_Stats.Value.HitInvulnerabilityTime;
                AddInvulnerability();
            }
        }
        OnAttackReceived?.Invoke();
        return ResolvedAttackThisFrame.Result;
    }

    private void Awake()
    {
        m_Health = GetComponent<HealthComponent>();
        m_Faction = GetComponent<FactionComponent>();
        m_Hurtboxes = GetComponentsInChildren<Hurtbox>();
        foreach (var hurtbox in m_Hurtboxes)
        {
            hurtbox.SetAttackTarget(this);
        }
    }

    private void Update()
    {
        if (m_HitInvulnerabilityTimer > 0f)
        {
            m_HitInvulnerabilityTimer -= Time.deltaTime;
            if (m_HitInvulnerabilityTimer <= 0f)
            {
                RemoveInvulnerability();
            }
        }
    }

    private void LateUpdate()
    {
        ResolvedAttackThisFrame = null;
    }

    private bool IsBlockingAttack(AttackData attack)
    {
        if (!IsBlocking)
            return false;
        int directionToAttack = Mathf.CeilToInt(Mathf.Sign(attack.Position.x - transform.position.x));
        return directionToAttack == BlockingDirection;
    }

    public class ResolvedAttack
    {
        public readonly int Damage;
        public readonly KnockbackData Knockback;
        public readonly Vector2 Position;
        public readonly Vector2 Velocity;
        public readonly AttackResult Result;
        public ResolvedAttack(int damage, KnockbackData knockback, Vector2 position, Vector2 velocity, AttackResult result)
        {
            Damage = damage;
            Knockback = knockback;
            Position = position;
            Velocity = velocity;
            Result = result;
        }
    }
}