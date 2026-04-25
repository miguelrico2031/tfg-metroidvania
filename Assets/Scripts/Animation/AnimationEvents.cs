using System;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    public event Action OnAttackDrawCompleted;
    public event Action OnAttackStrikeCompleted;
    public event Action OnAttackWithdrawCompleted;
    public event Action OnStandCompleted;
    public event Action OnStartBlockCompleted;
    public event Action OnStopBlockCompleted;
    public void AttackDrawCompleted() => OnAttackDrawCompleted?.Invoke();
    public void AttackStrikeCompleted() => OnAttackStrikeCompleted?.Invoke();
    public void AttackWithdrawCompleted() => OnAttackWithdrawCompleted?.Invoke();
    public void StandCompleted() => OnStandCompleted?.Invoke();
    public void StartBlockCompleted() => OnStartBlockCompleted?.Invoke();
    public void StopBlockCompleted() => OnStopBlockCompleted?.Invoke();
}