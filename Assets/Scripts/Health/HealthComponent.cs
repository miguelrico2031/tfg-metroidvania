using System;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    public int CurrentHealth { get; private set; }
    public int MaxHealth => m_Stats.Value.MaxHealth;
    public event Action<int> OnHealthChanged;

    [SerializeField] private DataReference<IHealthStats> m_Stats;

    public bool TakeDamage(int damage)
    {
        int appliedDamage = Mathf.Min(damage, CurrentHealth);
        if (appliedDamage <= 0)
            return false;

        CurrentHealth -= appliedDamage;
        OnHealthChanged?.Invoke(-appliedDamage);
        return true;
    }

    public bool Heal(int heal)
    {
        int appliedHeal = Mathf.Min(heal, MaxHealth - CurrentHealth);
        if (appliedHeal <= 0 || CurrentHealth == 0)
            return false;

        CurrentHealth += appliedHeal;
        OnHealthChanged?.Invoke(appliedHeal);
        return true;
    }

    private void Awake()
    {
        CurrentHealth = MaxHealth;
    }
}
