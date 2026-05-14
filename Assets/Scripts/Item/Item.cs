using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(Interactable))]
public class Item : MonoBehaviour, IPoolable
{
    [field: SerializeField] public Type ItemType { get; private set; }
    public ObjectPool<GameObject> ObjectPool { get; set; }


    private Interactable m_Interactable;

    protected virtual void OnEnable()
    {
        m_Interactable = GetComponent<Interactable>();
        m_Interactable.OnInteract += PickUp;
    }

    protected virtual void OnDisable()
    {
        m_Interactable.OnInteract -= PickUp;
    }

    protected virtual void PickUp()
    {
        m_Interactable.OnInteract -= PickUp;
        Dispose().Forget();
    }

    protected virtual async UniTask Dispose()
    {
        await UniTask.Yield(PlayerLoopTiming.Update);
        if (ObjectPool != null)
        {
            ObjectPool.Release(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public enum Type
    {
        Heal
    }    
}