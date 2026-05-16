using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public interface IDistributeHealItemDropsService : ILevelService
{
    public void RegisterComponent(DropHealItemOnDeathComponent component);
}

public class DistributeHealItemDropsService : MonoBehaviour, IDistributeHealItemDropsService
{
    [SerializeField] private int m_AmountOfHealDropsInThisLevel;
    [SerializeField] private LevelServiceLocator m_LevelServiceLocator;

    private readonly List<DropHealItemOnDeathComponent> m_Components = new();

    public void RegisterComponent(DropHealItemOnDeathComponent component) => m_Components.Add(component);

    private void OnEnable()
    {
        m_LevelServiceLocator.RegisterService<IDistributeHealItemDropsService>(this);
    }

    private void OnDisable()
    {
        m_LevelServiceLocator.UnregisterService<IDistributeHealItemDropsService>(this);
    }

    private void Start()
    {
        DistributeHeals().Forget();
    }

    private async UniTask DistributeHeals()
    {
        await UniTask.NextFrame(); // Let all components register themselves on Start() frame, then distribute heals

        int componentsToDisable = m_Components.Count - m_AmountOfHealDropsInThisLevel;

        if (componentsToDisable <= 0)
            return;

        HashSet<int> disabledIndices = new();
        while(disabledIndices.Count < componentsToDisable)
        {
            int index = Random.Range(0, m_Components.Count);
            if (disabledIndices.Add(index))
            {
                m_Components[index].enabled = false;
            }
        }
    }
}
