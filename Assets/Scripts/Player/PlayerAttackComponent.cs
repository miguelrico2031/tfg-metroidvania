using UnityEngine;
using UnityEngine.Assertions;

public enum PlayerAttack
{
    Attack1 = 0,
    Attack2 = 1,
    None
}

public class PlayerAttackComponent : MonoBehaviour
{
    public int AttackCount => m_AttackHitboxes.Length;
    public PlayerAttack ActiveAttack { get; private set; } = PlayerAttack.None;

    [SerializeField] private Hitbox[] m_AttackHitboxes;
    [SerializeField] private PlayerStats m_Stats;

    private BufferingTimer m_AdvanceAttackBuffer;

    public void StartAttack()
    {
        Assert.AreEqual(ActiveAttack, PlayerAttack.None, "Previous active attack was not finished.");
        var newActiveAttack = m_AdvanceAttackBuffer.Consume() ? PlayerAttack.Attack2 : PlayerAttack.Attack1;
        ActiveAttack = newActiveAttack;
        m_AttackHitboxes[(int)ActiveAttack].SetActive(true);
    }

    public void FinishAttack()
    {
        m_AttackHitboxes[(int)ActiveAttack].SetActive(false);
        if (ActiveAttack is PlayerAttack.Attack1)
        {
            m_AdvanceAttackBuffer.Register();
        }
        ActiveAttack = PlayerAttack.None;
    }

    private void Awake()
    {
        Assert.AreEqual((int)PlayerAttack.None, m_AttackHitboxes.Length, "Missing PlayerAttack enum or hitbox.");
        foreach (var hitbox in m_AttackHitboxes)
        {
            hitbox.SetActive(false);
        }

        m_AdvanceAttackBuffer = new(() => m_Stats.AdvanceAttackBufferTime);
    }

    private void Update()
    {
        m_AdvanceAttackBuffer.Tick(Time.deltaTime);
    }
}