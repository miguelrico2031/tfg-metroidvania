using System;
using UnityEngine;
using UnityEngine.Assertions;

public class AttackTargetComponent : MonoBehaviour, IAttackTarget
{
    [field: SerializeField] public Faction Faction { get; private set; }
    public ResolvedAttack ResolvedAttackThisFrame { get; set; }

    private bool m_IsInvulnerable = false;
    private Hurtbox[] m_Hurtboxes;

    public void SetInvulnerable(bool invulnerable)
    {
        if (invulnerable == m_IsInvulnerable)
            return;

        m_IsInvulnerable = invulnerable;
        foreach(var hurtbox in m_Hurtboxes)
        {
            hurtbox.SetActive(!invulnerable);
        }
    }

    public AttackResult ResolveAttack(AttackData attack)
    {
        if (ResolvedAttackThisFrame is not null)
            return AttackResult.Missed;

        AttackResult result = AttackResult.Hit;
        ResolvedAttackThisFrame = new(attack, result);
        return result;
    }

    private void Awake()
    {
        m_Hurtboxes = GetComponentsInChildren<Hurtbox>();
        foreach(var hurtbox in m_Hurtboxes)
        {
            hurtbox.SetAttackTarget(this);
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