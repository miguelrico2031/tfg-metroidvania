using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private StateMachine m_StateMachine;

    void Awake()
    {
        m_StateMachine = new StateMachineBuilder()
            .AddState(new StateA(), true)
            .AddState(new StateB())
            .AddTransition<StateA, StateB>(new Condition(() => { Debug.Log($"{Time.realtimeSinceStartup} seconds"); return Time.realtimeSinceStartup > 4f; }))
            .Build();
    }

    private void Start()
    {
        m_StateMachine.Start();
    }

    void Update()
    {
        m_StateMachine.Update();
    }

    void FixedUpdate()
    {
        m_StateMachine.FixedUpdate();
    }
}
