using UnityEngine;
using UnityEngine.Pool;

public interface IPoolable
{
    public ObjectPool<GameObject> ObjectPool { get; set; }
    public void OnGet();
}