using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

public class AttackHitSparks : MonoBehaviour, IPoolable
{
    public ObjectPool<GameObject> ObjectPool { get; set; }

    [SerializeField] private Animator m_Animator;
    
    private static readonly int s_Sparks = Animator.StringToHash("Sparks");

    public void OnGet()
    {
        m_Animator.Play(s_Sparks);
        _ = AwaitAnimationAndRelease();
    }

    private async Task AwaitAnimationAndRelease()
    {
        await Task.Yield();
        float duration = m_Animator.GetCurrentAnimatorStateInfo(0).length;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            await Task.Yield();
        }

        ObjectPool.Release(gameObject);
    }
}