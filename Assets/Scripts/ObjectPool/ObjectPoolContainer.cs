using UnityEngine;
using UnityEngine.Pool;

public class ObjectPoolContainer : MonoBehaviour
{
    [SerializeField] private int m_DefautCapacity;
    [SerializeField] private int m_MaxSize;
    [SerializeField] private GameObject m_Prefab;

    private ObjectPool<GameObject> m_Pool;

    void Awake()
    {
        m_Pool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(m_Prefab),
            actionOnGet: obj => obj.SetActive(true),
            actionOnRelease: obj => obj.SetActive(false),
            actionOnDestroy: obj => Destroy(obj),
            collectionCheck: true,
            defaultCapacity: m_DefautCapacity,
            maxSize: m_MaxSize
        );
    }

    public GameObject Get()
    {
        GameObject obj = m_Pool.Get();
        if(obj.TryGetComponent<IPoolable>(out var poolable))
        {
            poolable.ObjectPool = m_Pool;
        }
        return obj;
    }
}