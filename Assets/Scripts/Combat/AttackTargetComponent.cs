using System;
using UnityEngine;
using UnityEngine.Assertions;

public class AttackTargetComponent : MonoBehaviour, IAttackTarget
{
    [field: SerializeField] public Faction Faction { get; private set; }
    public event Action<Attack, AttackResult> OnAttacked;
    public ResolvedAttack ResolvedAttackThisFrame { get; set; }
    private Hurtbox[] m_Hurtboxes;

    public AttackResult ResolveAttack(Attack attack)
    {
        if (ResolvedAttackThisFrame is not null)
            return AttackResult.Missed;

        AttackResult result = AttackResult.Hit;
        OnAttacked?.Invoke(attack, result);
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
        public readonly Attack Attack;
        public readonly AttackResult Result;
        public ResolvedAttack(Attack attack, AttackResult result)
        {
            Attack = attack;
            Result = result;
        }
    }
}