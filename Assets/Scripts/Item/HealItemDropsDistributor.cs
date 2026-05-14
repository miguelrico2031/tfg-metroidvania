using System.Collections.Generic;
using UnityEngine;

public class HealItemDropsDistributor : MonoBehaviour
{
    [SerializeField] private int m_AmountOfHealDropsInThisLevel;

    private void Start()
    {
        DistributeHeals();
    }

    private void DistributeHeals()
    {
        var components = FindObjectsByType<DropHealItemOnDeathComponent>(FindObjectsInactive.Include);
        int componentsToDisable = components.Length - m_AmountOfHealDropsInThisLevel;

        if (componentsToDisable <= 0)
            return;

        HashSet<int> disabledIndices = new();
        while(disabledIndices.Count < componentsToDisable)
        {
            int index = Random.Range(0, components.Length);
            if (disabledIndices.Add(index))
            {
                components[index].enabled = false;
            }
        }
    }
}
