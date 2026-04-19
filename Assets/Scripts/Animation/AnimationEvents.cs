using System;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    public event Action OnAttackDrawCompleted;
    public event Action OnAttackStrikeCompleted;
    public event Action OnAttackWithdrawCompleted;
    public void AttackDrawCompleted() => OnAttackDrawCompleted?.Invoke();
    public void AttackStrikeCompleted() => OnAttackStrikeCompleted?.Invoke();
    public void AttackWithdrawCompleted() => OnAttackWithdrawCompleted?.Invoke();
}