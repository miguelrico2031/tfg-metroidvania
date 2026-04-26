using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private float m_LoseHealthSpeed;
    [SerializeField] private Slider m_Bar;
    [SerializeField] private HealthComponent m_PlayerHealth;

    private Tween m_HealthChangeTween;

    private void OnEnable()
    {
        m_PlayerHealth.OnHealthChanged += OnHealthChanged;
    }
    private void OnDisable()
    {
        m_PlayerHealth.OnHealthChanged -= OnHealthChanged;
    }

    private void Start()
    {
        m_Bar.minValue = 0f;
        m_Bar.maxValue = m_Bar.value = m_PlayerHealth.MaxHealth;
        m_Bar.value = m_PlayerHealth.MaxHealth;
    }

    private void OnHealthChanged(int healthChange)
    {
        m_HealthChangeTween?.Kill();
        if(healthChange > 0)
        {
            m_Bar.value = m_PlayerHealth.CurrentHealth;
        }
        else
        {
            float tweenDuration = Mathf.Abs(m_Bar.value - m_PlayerHealth.CurrentHealth) / m_LoseHealthSpeed;
            m_HealthChangeTween = DOTween.To(() => m_Bar.value, x => m_Bar.value = x, m_PlayerHealth.CurrentHealth, tweenDuration);
        }
    }
}