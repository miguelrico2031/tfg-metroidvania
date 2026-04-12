using System;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    public event Action OnPlayerAttack1Completed;
    public event Action OnPlayerAttack1Withdrawn;
    public event Action OnPlayerAttack2Completed;
    public event Action OnPlayerAttack2Withdrawn;

    public void PlayerAttack1Completed() => OnPlayerAttack1Completed?.Invoke();
    public void PlayerAttack1Withdrawn() => OnPlayerAttack1Withdrawn?.Invoke();
    public void PlayerAttack2Completed() => OnPlayerAttack2Completed?.Invoke();
    public void PlayerAttack2Withdrawn() => OnPlayerAttack2Withdrawn?.Invoke();
}