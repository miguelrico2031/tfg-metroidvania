using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStaminaBar : MonoBehaviour
{
    [SerializeField] private Color m_DefaultColor;
    [SerializeField] private Color m_ExhaustedColor;
    [SerializeField] private Slider m_Bar;
    [SerializeField] private Image m_BarFillImage;
    [SerializeField] private PlayerStaminaComponent m_PlayerStamina;

    private bool m_WasExhausted = false;

    private void OnEnable()
    {
        m_PlayerStamina.OnStaminaLost += OnStaminaLost;
        m_PlayerStamina.OnStaminaRecoverProgress += OnStaminaRecoverProgress;
    }
    private void OnDisable()
    {
        m_PlayerStamina.OnStaminaLost -= OnStaminaLost;
        m_PlayerStamina.OnStaminaRecoverProgress -= OnStaminaRecoverProgress;
    }

    private void Start()
    {
        m_Bar.minValue = 0f;
        m_Bar.maxValue = m_Bar.value = m_PlayerStamina.MaxStamina;
        m_Bar.value = m_PlayerStamina.MaxStamina;
        m_BarFillImage.color = m_DefaultColor;
    }

    private void OnStaminaLost(int staminaLost)
    {
        m_Bar.value = m_PlayerStamina.CurrentStamina; //Loss is instant
    }
    private void OnStaminaRecoverProgress(float progress)
    {
        //Change color only when state changes
        if (m_WasExhausted && !m_PlayerStamina.IsExhausted)
        {
            m_BarFillImage.color = m_DefaultColor;
        }
        else if (!m_WasExhausted && m_PlayerStamina.IsExhausted)
        {
            m_BarFillImage.color = m_ExhaustedColor;
        }
        m_WasExhausted = m_PlayerStamina.IsExhausted;

        m_Bar.value = m_PlayerStamina.CurrentStamina + progress;
    }
}