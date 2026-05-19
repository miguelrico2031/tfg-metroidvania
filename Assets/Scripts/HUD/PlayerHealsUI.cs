using UnityEngine;
using UnityEngine.UI;

public class PlayerHealsUI : MonoBehaviour
{
    [SerializeField] private HealComponent m_PlayerHeal;

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
        m_Heals = new GameObject[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            m_Heals[i] = transform.GetChild(i).gameObject;
            if(i < m_PlayerHeal.MaxHeals)
            {
                m_Heals[i].transform.GetChild(0).gameObject.SetActive(false);
            }
            else
            {
                m_Heals[i].SetActive(false);
            } 
        }
    }

    private void Start()
    {
        m_LastActiveHealIndex = m_PlayerHeal.CurrentHeals - 1;
        for (int i = 0; i < m_PlayerHeal.CurrentHeals; i++)
        {
            m_Heals[i].transform.GetChild(0).gameObject.SetActive(true);
        }
    }

    private void OnHealAdded()
    {
        m_Heals[++m_LastActiveHealIndex].transform.GetChild(0).gameObject.SetActive(true);
    }

    private void OnHealConsumed()
    {
        m_Heals[m_LastActiveHealIndex--].transform.GetChild(0).gameObject.SetActive(false);
    }
}