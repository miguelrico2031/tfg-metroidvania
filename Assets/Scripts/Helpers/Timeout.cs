using System;
using UnityEngine;

public class Timeout : MonoBehaviour
{
    private float m_Timer = 0f;
    private Action m_Callback;
    public void StartTimeout(float delay, Action callback)
    {
        m_Timer = delay;
        m_Callback = callback;
    }

    private void Update()
    {
        if(m_Timer > 0f)
        {
            m_Timer -= Time.deltaTime;
            if(m_Timer <= 0f)
            {
                m_Callback.Invoke();
                Destroy(gameObject);
            }
        }
    }
}