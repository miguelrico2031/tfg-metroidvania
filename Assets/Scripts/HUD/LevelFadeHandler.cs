using UnityEngine;

public class LevelFadeHandler : MonoBehaviour
{
    [SerializeField] private FadeUI m_FadeUI;
    [SerializeField] private LevelLoader m_LevelLoader;

    private void OnEnable()
    {
        m_LevelLoader.OnLevelLoadStarted += OnLevelLoadStarted;
    }

    private void OnDisable()
    {
        m_LevelLoader.OnLevelLoadStarted -= OnLevelLoadStarted;
    }

    private void Start()
    {
        m_FadeUI.FadeIn(m_LevelLoader.ExitLevelDelay);
    }

    private void OnLevelLoadStarted(string level, LevelLoader.Reason reason)
    {
        if (reason is LevelLoader.Reason.DoorTriggered)
        {
            m_FadeUI.FadeOut(m_LevelLoader.ExitLevelDelay);
        }
    }
}