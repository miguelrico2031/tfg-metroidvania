using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

public class AttackHitSparks : MonoBehaviour, IPoolable
{
    public ObjectPool<GameObject> ObjectPool { get; set; }

    [SerializeField] private Animator m_Animator;
    
    private static readonly int s_Sparks = Animator.StringToHash("Sparks");

    private void OnEnable()
    {
        m_Animator.Play(s_Sparks);
        _ = AwaitAnimationAndRelease();
    }

    private async UniTask AwaitAnimationAndRelease()
    {
        await UniTask.Yield();
        float duration = m_Animator.GetCurrentAnimatorStateInfo(0).length;
        await UniTask.Delay(TimeSpan.FromSeconds(duration), ignoreTimeScale: false);
        ObjectPool.Release(gameObject);
    }
}