using UnityEngine;
using UnityEngine.Assertions;

public enum Faction
{
    Player,
    Enemy,
    MAX
}

[CreateAssetMenu(menuName = "ScriptableObjects/FactionsData")]
public class FactionsData : ScriptableObject
{
    [SerializeField] private bool[] m_FactionMatrix = new bool [k_FactionCount * k_FactionCount];

    private const int k_FactionCount = (int) Faction.MAX;

    public bool IsHostileTo(Faction attacker, Faction target)
    {
        Assert.AreNotEqual(attacker, Faction.MAX, "Wrong faction for attacker.");
        Assert.AreNotEqual(target, Faction.MAX, "Wrong faction for target.");
        int index = (int)attacker * k_FactionCount + (int)target;
        return index >= 0 && index < m_FactionMatrix.Length && m_FactionMatrix[index];
    }

#if UNITY_EDITOR
    public int FactionCount => k_FactionCount;
    public bool GetRelation(int attacker, int target)
        => m_FactionMatrix[attacker * k_FactionCount + target];
    public void SetRelation(int attacker, int target, bool value)
        => m_FactionMatrix[attacker * k_FactionCount + target] = value;
#endif
}