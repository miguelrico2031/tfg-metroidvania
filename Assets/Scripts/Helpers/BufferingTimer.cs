using System;

public class BufferingTimer
{
    private float m_Timer = 0f;
    private readonly Func<float> m_TimeGetter;
    public BufferingTimer(Func<float> timeGetter) { m_TimeGetter = timeGetter; }
    public void Register() => m_Timer = m_TimeGetter();
    public bool Check() => m_Timer > 0f;
    public void Clear() => m_Timer = 0f;
    public bool Consume()
    {
        bool check = Check();
        Clear();
        return check;
    }
    public void Tick(float dt)
    {
        if (m_Timer > 0)
        {
            m_Timer -= dt;
        }
    }
}