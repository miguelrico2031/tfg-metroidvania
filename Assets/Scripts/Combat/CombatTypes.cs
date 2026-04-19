using System;
using UnityEngine;

public interface ICombatStats
{
    public float HitInvulnerabilityTime { get; }
    public float AdvanceAttackComboBufferTime { get; }
    public int AttackComboCount { get; }
}

public interface IAttackSource
{
    public Vector2 Position { get; }
    public Faction Faction { get; }
}

public interface IAttackTarget
{
    public Faction Faction { get; }
    public AttackResult ResolveAttack(AttackData attack);
}

public enum AttackResult
{
    Missed,
    Blocked,
    Parried,
    Hit
}

[Serializable]
public struct KnockbackData
{
    public float Duration;
    public float Distance;
    public float Height;
}

[Serializable]
public struct AttackData
{
    public int Damage;
    public KnockbackData Knockback;
    public IAttackSource Source;
}

