using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class FadeUI : MonoBehaviour
{
    [SerializeField] private Ease m_FadeEase;
    private CanvasGroup m_CanvasGroup;

    public void FadeIn(float time)
    {
        if(m_CanvasGroup.DOKill(complete: false) == 0)
        {
            m_CanvasGroup.alpha = 1f;
        }
        m_CanvasGroup.DOFade(0f, time).SetEase(m_FadeEase);
    }

    public void FadeOut(float time)
    {
        if (m_CanvasGroup.DOKill(complete: false) == 0)
        {
            m_CanvasGroup.alpha = 0f;
        }
        m_CanvasGroup.DOFade(1f, time).SetEase(m_FadeEase);
    }

    private void Awake()
    {
        m_CanvasGroup = GetComponent<CanvasGroup>();
    }
}