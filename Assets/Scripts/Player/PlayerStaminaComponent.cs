using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerStaminaComponent : MonoBehaviour
{
    public event Action<int> OnStaminaLost;
    public event Action<float> OnStaminaRecoverProgress;
    public int CurrentStamina { get; private set; }
    public int MaxStamina => m_Stats.MaxStamina;
    public float RecoveryPointTime { get; private set; }
    public bool IsExhausted { get; private set; }

    [SerializeField] private PlayerStats m_Stats;

    private Dictionary<StaminaAction, StaminaActionData> m_StaminaActions;
    private float m_RecoveryPointTimer;

    public bool CanPerformAction(StaminaAction action)
    {
        return !IsExhausted || m_StaminaActions[action].IsInvoluntary;
    }

    public void RegisterActionPerformed(StaminaAction action)
    {
        Assert.IsTrue(CanPerformAction(action), $"Performing unallowed action: {action}.");

        int cost = m_StaminaActions[action].Cost;
        int previousStamina = CurrentStamina;
        CurrentStamina = Mathf.Max(CurrentStamina - cost, 0);
        IsExhausted = CurrentStamina == 0; //Running out of stamina causes exhaustion, so player cannot perform actions until stamina is full
        int staminaLost = previousStamina - CurrentStamina;
        if(staminaLost > 0)
        {
            OnStaminaLost?.Invoke(staminaLost);
            m_RecoveryPointTimer = 0f;
        }
    }

    private void Awake()
    {
        m_StaminaActions = new(m_Stats.StaminaActions.Select(s => new KeyValuePair<StaminaAction, StaminaActionData>(s.Action, s)));
        CurrentStamina = MaxStamina;
        RecoveryPointTime = 1f / m_Stats.StaminaRecoveryRate;
        m_RecoveryPointTimer = 0f;
    }

    private void Update()
    {
        if (CurrentStamina < m_Stats.MaxStamina)
        {
            RecoverStamina();
        }
    }

    private void RecoverStamina()
    {
        if (m_RecoveryPointTimer < RecoveryPointTime)
        {
            m_RecoveryPointTimer += Time.deltaTime;
        }
        if (m_RecoveryPointTimer >= RecoveryPointTime)
        {
            CurrentStamina++;
            m_RecoveryPointTimer = 0f;
            if (CurrentStamina == MaxStamina)
            {
                IsExhausted = false;
            }
        }
        OnStaminaRecoverProgress.Invoke(m_RecoveryPointTimer / RecoveryPointTime);
    }
}