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

    private Dictionary<PlayerStaminaAction, int> m_StaminaActionCosts;
    private float m_RecoveryPointTimer;

    public bool CanPerformAction(PlayerStaminaAction action)
    {
        return !IsExhausted;
    }

    public void RegisterActionPerformed(PlayerStaminaAction action)
    {
        Assert.IsTrue(CanPerformAction(action), $"Performing unallowed action: {action}.");

        int cost = m_StaminaActionCosts[action];
        int previousStamina = CurrentStamina;
        CurrentStamina = Mathf.Max(CurrentStamina - cost, 0);
        IsExhausted = CurrentStamina == 0; //Running out of stamina causes exhaustion, so player cannot perform actions until stamina is full
        OnStaminaLost?.Invoke(previousStamina - CurrentStamina);
    }

    private void Awake()
    {
        m_StaminaActionCosts =
            new(m_Stats.StaminaActionCosts.Select(entry => new KeyValuePair<PlayerStaminaAction, int>(entry.Action, entry.Cost)));
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