using System;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent (typeof(HealthComponent))]
public class HealComponent : MonoBehaviour
{
    public event Action OnHealAdded;
    public event Action OnHealConsumed;
    public int CurrentHeals { get; private set; } = 0;
    public int MaxHeals => m_Stats.Value.MaxHeals;
    public bool HasFullHeals => CurrentHeals == MaxHeals;

    [SerializeField] private DataReference<IHealthStats> m_Stats;
    [SerializeField] private DataReference<IPersistence> m_Persistence;

    private HealthComponent m_Health;


    public void AddHeal()
    {
        Assert.IsFalse(HasFullHeals, "Cannot add add heals if has full heals.");
        CurrentHeals++;
        SetPersistentData();
        OnHealAdded?.Invoke();
    }

    public void ConsumeHeal()
    {
        Assert.IsTrue(CurrentHeals > 0, "Cannot consume heal if heals are empty.");
        CurrentHeals--;
        m_Health.Heal(m_Stats.Value.HealAmount);
        SetPersistentData();
        OnHealConsumed?.Invoke();
    }

    private void Awake()
    {
        m_Health = GetComponent<HealthComponent>();
        if(m_Persistence.Value.TryGetEntry(PersistentData.PlayerHeals, out string healsStr) && 
            int.TryParse(healsStr, out int heals))
        {
            CurrentHeals = heals;
        }
    }

    private void SetPersistentData()
    {
        m_Persistence.Value.SetEntry(PersistentData.PlayerHeals, CurrentHeals.ToString());
    }
}