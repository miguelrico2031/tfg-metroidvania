using UnityEngine;

[RequireComponent(typeof(Checkpoint), typeof(Interactable))]
public class BonfireCheckpoint : MonoBehaviour
{
    [SerializeField] private Animator m_Animator;
    [SerializeField] private GameObject m_Smoke;

    private static readonly int m_Light = Animator.StringToHash("Light");
    private static readonly int m_Idle = Animator.StringToHash("Idle");

    private Interactable m_Interactable;
    private Checkpoint m_Checkpoint;

    private void Awake()
    {
        m_Interactable = GetComponent<Interactable>();
        m_Checkpoint = GetComponent<Checkpoint>();
    }

    private void Start()
    {
        if(m_Checkpoint.IsCheckpointUnlocked())
        {
            m_Smoke.SetActive(true);
            if (m_Checkpoint.IsCheckpointActive())
            {
                m_Animator.SetTrigger(m_Idle);
                m_Interactable.enabled = false;
                return;
            }
        }
        m_Interactable.OnInteract += OnInteract;
    }

    private void OnInteract()
    {
        m_Interactable.OnInteract -= OnInteract;
        m_Interactable.enabled = false;
        m_Checkpoint.SetAsActiveCheckpoint();
        m_Animator.SetTrigger(m_Light);
        m_Smoke.SetActive(true);
    }
}