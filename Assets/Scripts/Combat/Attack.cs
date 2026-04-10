using System;
using UnityEngine;

public interface IAttackSource
{
    public Vector2 Position { get; }
    public Faction Faction { get; }
    public event Action<Attack, AttackResult> OnAttack;
}

public interface IAttackTarget
{
    public Faction Faction { get; }
    public event Action<Attack, AttackResult> OnAttacked;
    public AttackResult ResolveAttack(Attack attack);
}

public enum AttackResult
{
    Missed,
    Blocked,
    Parried,
    Hit
}

[Serializable]
public struct Attack
{
    public int Damage;
    public float KnockbackDistance;
    public float KnockbackDuration;
    public IAttackSource Source;
}

