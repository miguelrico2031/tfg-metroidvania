using UnityEngine;
using UnityEngine.UI;

public class PlayerHealsUI : MonoBehaviour
{
    [SerializeField] private HealComponent m_PlayerHeal;
    [SerializeField] private GameObject m_HealPrefab;

    private GameObject[] m_Heals;
    private int m_LastActiveHealIndex = -1;

    private void OnEnable()
    {
        m_PlayerHeal.OnHealAdded += OnHealAdded;
        m_PlayerHeal.OnHealConsumed += OnHealConsumed;
    }

    private void OnDisable()
    {
        m_PlayerHeal.OnHealAdded -= OnHealAdded;
        m_PlayerHeal.OnHealConsumed -= OnHealConsumed;
    }

    private void Awake()
    {
        m_Heals = new GameObject[m_PlayerHeal.MaxHeals];
        for (int i = 0; i < m_PlayerHeal.MaxHeals; i++)
        {
            m_Heals[i] = Instantiate(m_HealPrefab, transform);
            m_Heals[i].SetActive(false);
        }
    }

    private void Start()
    {
        m_LastActiveHealIndex = m_PlayerHeal.CurrentHeals - 1;
        for (int i = 0; i < m_PlayerHeal.CurrentHeals; i++)
        {
            m_Heals[i].SetActive(true);
        }
    }

    private void OnHealAdded()
    {
        m_Heals[++m_LastActiveHealIndex].SetActive(true);
    }

    private void OnHealConsumed()
    {
        m_Heals[m_LastActiveHealIndex--].SetActive(false);
    }
}