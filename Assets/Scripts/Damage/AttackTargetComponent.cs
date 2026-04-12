using System;
using UnityEngine;
using UnityEngine.Assertions;

public class AttackTargetComponent : MonoBehaviour, IAttackTarget
{
    [field: SerializeField] public Faction Faction { get; private set; }

    public ResolvedAttack ResolvedAttackThisFrame { get; set; }
    public event Action OnAttackReceived;
    public bool IsInvulnerable => m_InvulnerabilitySources > 0;
    public bool IsAlive => m_Health is { CurrentHealth: > 0 };

    [SerializeField] private float m_HitInvulnerabilityTime;

    private int m_InvulnerabilitySources = 0;
    private float m_HitInvulnerabilityTimer = 0f;
    private HealthComponent m_Health;
    private Hurtbox[] m_Hurtboxes;

    public void SetInvulnerable(bool invulnerable)
    {
        Assert.IsFalse(!IsInvulnerable && !invulnerable, "Tried to clear invulnerability without having set it.");
        bool wasInvulnerable = IsInvulnerable;
        m_InvulnerabilitySources += invulnerable ? 1 : -1;
        m_InvulnerabilitySources = Math.Max(m_InvulnerabilitySources, 0);

        if (wasInvulnerable == IsInvulnerable)
            return;

        foreach (var hurtbox in m_Hurtboxes)
        {
            hurtbox.SetActive(!IsInvulnerable);
        }
    }

    public AttackResult ResolveAttack(AttackData attack)
    {
        if (ResolvedAttackThisFrame is not null || IsInvulnerable || !IsAlive)
            return AttackResult.Missed;

        AttackResult result = AttackResult.Hit;
        ResolvedAttackThisFrame = new(attack, result);
        if(m_Health != null)
        {
            m_Health.TakeDamage(attack.Damage);
        }
        if (IsAlive && m_HitInvulnerabilityTime > 0.01f)
        {
            m_HitInvulnerabilityTimer = m_HitInvulnerabilityTime;
            SetInvulnerable(true);
        }
        OnAttackReceived?.Invoke();
        return result;
    }

    private void Awake()
    {
        m_Health = GetComponent<HealthComponent>();
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
                SetInvulnerable(false);
            }
        }
    }

    private void LateUpdate()
    {
        ResolvedAttackThisFrame = null;
    }

    public class ResolvedAttack
    {
        public readonly AttackData Attack;
        public readonly AttackResult Result;
        public ResolvedAttack(AttackData attack, AttackResult result)
        {
            Attack = attack;
            Result = result;
        }
    }
}