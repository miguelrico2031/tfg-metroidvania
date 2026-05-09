using System;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    public event Action<AnimationEventType> OnEventReceived;
    public void AttackDrawCompleted() => OnEventReceived?.Invoke(AnimationEventType.AttackDrawCompleted);
    public void AttackStrikeCompleted() => OnEventReceived?.Invoke(AnimationEventType.AttackStrikeCompleted);
    public void AttackWithdrawCompleted() => OnEventReceived?.Invoke(AnimationEventType.AttackWithdrawCompleted);
    public void StandCompleted() => OnEventReceived?.Invoke(AnimationEventType.StandCompleted);
    public void StartBlockCompleted() => OnEventReceived?.Invoke(AnimationEventType.StartBlockCompleted);
    public void StopBlockCompleted() => OnEventReceived?.Invoke(AnimationEventType.StopBlockCompleted);
    public void PickUpCompleted() => OnEventReceived?.Invoke(AnimationEventType.PickUpCompleted);
    public void HealCompleted() => OnEventReceived?.Invoke(AnimationEventType.HealCompleted);
    public void CastStrikeCompleted() => OnEventReceived?.Invoke(AnimationEventType.CastStrikeCompleted);
    public void CastCompleted() => OnEventReceived?.Invoke(AnimationEventType.CastCompleted);
}