using UnityEngine;
using UnityEngine.Assertions;

public enum PlayerAttack
{
    Attack1 = 0,
    Attack2 = 1,
}

public class PlayerAttackComponent : MonoBehaviour
{
    public int AttackCount => m_AttackHitboxes.Length;
    public PlayerAttack? ActiveAttack { get; private set; }

    [SerializeField] private Hitbox[] m_AttackHitboxes;

    public void StartAttack(PlayerAttack attack)
    {
        Assert.IsTrue(ActiveAttack is null, $"Tried to start attack while other attack ({ActiveAttack}) was active.");        
        ActiveAttack = attack;
        m_AttackHitboxes[(int)ActiveAttack].SetActive(true);
    }

    public void FinishAttack(PlayerAttack attack)
    {
        Assert.AreEqual(ActiveAttack, attack, $"Tried to finish an attack ({attack}) that was not started.");
        m_AttackHitboxes[(int)ActiveAttack].SetActive(false);
        ActiveAttack = null;
    }

    private void Awake()
    {
        Assert.AreEqual(System.Enum.GetValues(typeof(PlayerAttack)).Length, m_AttackHitboxes.Length, "Missing PlayerAttack enum or hitbox.");
        foreach(var  hitbox in m_AttackHitboxes)
        {
            hitbox.SetActive(false);
        }
    }
}