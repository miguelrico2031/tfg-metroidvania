using System;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(FactionComponent))]
public class EnemyTargetterComponent : MonoBehaviour
{
    public AttackTargetComponent ActiveTarget { get; private set; }

    [SerializeField] private AIBounds m_Bounds;
    [SerializeField] private EnemyStats m_Stats;
    [SerializeField] private FactionsData m_FactionsData;

    private FactionComponent m_Faction;

    private void Awake()
    {
        m_Faction = GetComponent<FactionComponent>();    
    }

    private void OnEnable()
    {
        m_Bounds.OnIntruderEnterBounds += OnIntruderEnterBounds;
        m_Bounds.OnIntruderLeaveBounds += OnIntruderLeaveBounds;
    }

    private void OnDisable()
    {
        m_Bounds.OnIntruderEnterBounds -= OnIntruderEnterBounds;
        m_Bounds.OnIntruderLeaveBounds -= OnIntruderLeaveBounds;
    }

    private void OnIntruderEnterBounds(Collider2D intruder)
    {
        if(intruder.TryGetComponent<AttackTargetComponent>(out var intruderTarget) &&
            m_FactionsData.IsHostileTo(m_Faction.Faction, intruderTarget.Faction))
        {
            ActiveTarget = intruderTarget;
        }
    }

    private void OnIntruderLeaveBounds(Collider2D intruder)
    {
        if (intruder.TryGetComponent<AttackTargetComponent>(out var intruderTarget) &&
            intruderTarget == ActiveTarget)
        {
            Collider2D possibleNewTarget = m_Bounds.IntrudersInsideBounds.FirstOrDefault();

            ActiveTarget = (possibleNewTarget != null &&
                possibleNewTarget.TryGetComponent<AttackTargetComponent>(out var newIntruderTarget) &&
                m_FactionsData.IsHostileTo(m_Faction.Faction, newIntruderTarget.Faction))
                ? newIntruderTarget
                : null;
        }
    }
}