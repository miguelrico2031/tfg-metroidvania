using System;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(FactionComponent))]
public class AITargetterComponent : MonoBehaviour
{
    public AttackTargetComponent ActiveTarget { get; private set; }

    [SerializeField] private EnemyStats m_Stats;
    [SerializeField] private FactionsData m_FactionsData;

    private AIBounds m_Bounds;
    private FactionComponent m_Faction;

    private void OnEnable()
    {
        m_Faction = GetComponent<FactionComponent>();
        m_Bounds = LocateBounds();
        if (m_Bounds)
        {
            m_Bounds.OnIntruderEnterBounds += OnIntruderEnterBounds;
            m_Bounds.OnIntruderLeaveBounds += OnIntruderLeaveBounds;
        }
    }

    private void OnDisable()
    {
        if (m_Bounds)
        {
            m_Bounds.OnIntruderEnterBounds -= OnIntruderEnterBounds;
            m_Bounds.OnIntruderLeaveBounds -= OnIntruderLeaveBounds;
        }
    }

    private AIBounds LocateBounds()
    {
        Collider2D[] overlaps = Physics2D.OverlapPointAll(transform.position);
        foreach (var overlap in overlaps)
        {
            if (overlap.TryGetComponent<AIBounds>(out var aiBounds))
                return aiBounds;
        }
        return null;
    }

    private void OnIntruderEnterBounds(Collider2D intruder)
    {
        if (intruder.TryGetComponent<AttackTargetComponent>(out var intruderTarget) &&
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