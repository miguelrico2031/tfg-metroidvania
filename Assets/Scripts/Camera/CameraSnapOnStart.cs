using Unity.Cinemachine;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class CinemachineSnapOnStart : MonoBehaviour
{
    [SerializeField] private CinemachineCamera m_Camera;
    [SerializeField] private CinemachineConfiner2D m_Confiner;

    private Vector3 m_TargetAwakePos;
    private void Awake()
    {
        m_TargetAwakePos = m_Camera.Follow.position;
    }

    private void Start()
    {
        SnapOnStartAsync().Forget();
    }
    
    private async UniTaskVoid SnapOnStartAsync()
    {
        await UniTask.WaitForFixedUpdate(destroyCancellationToken);
        await UniTask.WaitUntil(() => m_Confiner.BoundingShapeIsBaked, cancellationToken: destroyCancellationToken);
        m_Camera.OnTargetObjectWarped(m_Camera.Follow,m_Camera.Follow.position - m_TargetAwakePos);
        m_Camera.CancelDamping();
    }
}