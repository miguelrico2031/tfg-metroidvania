using DG.Tweening;
using System;
using Unity.Cinemachine;
using UnityEngine;

public class CameraVerticalControl : MonoBehaviour
{
    [SerializeField] private float m_DampingOnFalling;
    [SerializeField] private float m_DampingTweenTime;
    [SerializeField] private PlayerStateComponent m_PlayerState;
    [SerializeField] private CinemachinePositionComposer m_PositionComposer;

    private float m_DampingOnRising;
    private bool m_WasFalling;
    private Tween m_Tween;

    private void OnEnable()
    {
        m_DampingOnRising = m_PositionComposer.Damping.y;
        m_PlayerState.OnStateChanged += OnStateChanged;
    }

    private void OnDisable()
    {
        m_PlayerState.OnStateChanged -= OnStateChanged;
    }

    private void OnStateChanged(Type state)
    {
        if (state == typeof(PlayerFallingState))
        {
            TweenYDamping(m_DampingOnFalling);
            m_WasFalling = true;
        }
        else if (m_WasFalling)
        {
            TweenYDamping(m_DampingOnRising);
            m_WasFalling = false;
        }

    }

    private void TweenYDamping(float targetYDamping)
    {
        Vector3 targetDamping = m_PositionComposer.Damping;
        targetDamping.y = targetYDamping;
        if (m_Tween != null && m_Tween.IsActive())
        {
            m_Tween.Kill(true);
        }
        m_Tween = DOTween.To(() => m_PositionComposer.Damping, x => m_PositionComposer.Damping = x, targetDamping, m_DampingTweenTime);
    }
}