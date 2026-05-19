using UnityEngine;
using UnityEngine.Assertions;

public enum Faction
{
    Player,
    Enemy,
    AllHostile,
    AllFriendly,
    MAX
}

[CreateAssetMenu(menuName = "ScriptableObjects/FactionsData")]
public class FactionsData : ScriptableObject
{
    [SerializeField] private bool[] m_FactionMatrix = new bool [c_FactionCount * c_FactionCount];

    private const int c_FactionCount = (int) Faction.MAX;

    public bool IsHostileTo(Faction attacker, Faction target)
    {
        Assert.AreNotEqual(attacker, Faction.MAX, "Wrong faction for attacker.");
        Assert.AreNotEqual(target, Faction.MAX, "Wrong faction for target.");
        int index = (int)attacker * c_FactionCount + (int)target;
        return index >= 0 && index < m_FactionMatrix.Length && m_FactionMatrix[index];
    }

#if UNITY_EDITOR
    public int FactionCount => c_FactionCount;
    public bool GetRelation(int attacker, int target)
        => m_FactionMatrix[attacker * c_FactionCount + target];
    public void SetRelation(int attacker, int target, bool value)
        => m_FactionMatrix[attacker * c_FactionCount + target] = value;

    private void OnValidate()
    {
        if(m_FactionMatrix.Length < c_FactionCount * c_FactionCount)
        {
            m_FactionMatrix = new bool[c_FactionCount * c_FactionCount];
        }
    }
#endif
}