using UnityEngine;
using UnityEngine.Assertions;


public class AttackComboComponent : MonoBehaviour
{
    //ActiveAttack = 0 means no attack, first attack is 1, second is 2, etc.
    public int ActiveAttack { get; private set; } = 0;

    [SerializeField] private StatsReference<ICombatStats> m_Stats;

    private BufferingTimer m_AdvanceAttackBuffer;
    private int m_NextAttackInCombo = 0;

    public void StartAttack()
    {
        Assert.AreEqual(ActiveAttack, 0, "Previous active attack was not finished.");
        ActiveAttack = m_AdvanceAttackBuffer.Consume() ? m_NextAttackInCombo : 1;
        m_NextAttackInCombo = 0;
    }

    public void FinishAttack()
    {
        if (ActiveAttack > 0 && ActiveAttack < m_Stats.Value.AttackComboCount)
        {
            m_AdvanceAttackBuffer.Register();
            m_NextAttackInCombo = ActiveAttack + 1;
        }
        ActiveAttack = 0;
    }

    private void Awake()
    {
        Assert.IsTrue(m_Stats.Value.AttackComboCount > 1, "AttackComboComponent with no attack combo in stats.");
        m_AdvanceAttackBuffer = new(() => m_Stats.Value.AdvanceAttackComboBufferTime);
    }

    private void Update()
    {
        m_AdvanceAttackBuffer.Tick(Time.deltaTime);
    }
}