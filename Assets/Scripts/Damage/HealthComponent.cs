using System;
using UnityEngine;

public interface IHealthStats
{
    public int MaxHealth { get; }
}

public class HealthComponent : MonoBehaviour
{
    public int CurrentHealth { get; private set; }
    public int MaxHealth => m_Stats.Value.MaxHealth;
    public event Action<int> OnHealthChanged;

    [SerializeField] private StatsReference<IHealthStats> m_Stats;

    public void TakeDamage(int damage)
    {
        int appliedDamage = Mathf.Min(damage, CurrentHealth);
        if (appliedDamage <= 0)
            return;

        CurrentHealth -= appliedDamage;
        OnHealthChanged?.Invoke(-appliedDamage);
    }

    private void Awake()
    {
        CurrentHealth = MaxHealth;
    }
}
