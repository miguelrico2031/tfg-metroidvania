using UnityEngine;

public interface IObjectPoolService : ILevelService
{
    public ObjectPoolContainer GetProjectilesPoolContainer();
    public ObjectPoolContainer GetAttackHitSparksPoolContainer();
    public ObjectPoolContainer GetHealsPoolContainer();
}

public class ObjectPoolService : MonoBehaviour, IObjectPoolService
{
    [SerializeField] private ObjectPoolContainer m_ProjectilesPoolContainer;
    [SerializeField] private ObjectPoolContainer m_AttackHitSparksPoolContainer;
    [SerializeField] private ObjectPoolContainer m_HealsPoolContainer;
    [SerializeField] private LevelServiceLocator m_LevelServiceLocator;

    public ObjectPoolContainer GetProjectilesPoolContainer() => m_ProjectilesPoolContainer;
    public ObjectPoolContainer GetAttackHitSparksPoolContainer() => m_AttackHitSparksPoolContainer;
    public ObjectPoolContainer GetHealsPoolContainer() => m_HealsPoolContainer;

    private void OnEnable()
    {
        m_LevelServiceLocator.RegisterService<IObjectPoolService>(this);
    }

    private void OnDisable()
    {
        m_LevelServiceLocator.UnregisterService<IObjectPoolService>(this);
    }

}
