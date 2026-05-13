using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

public class DialogueDisplayer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_Label;
    [SerializeField] private float m_CharactersPerSecond;
    [SerializeField] private float m_HideDuration;
    [SerializeField] private Ease m_HideEase = Ease.InQuad;

    private LocalizedString m_CurrentSentence;
    private bool m_IsComplete;

    private UniTaskCompletionSource m_TypewriterCompletion;
    private CancellationTokenSource m_TypewriterCts;
    private CancellationTokenSource m_HideCts;


    public async UniTask DisplaySentenceAsync(LocalizedString sentence)
    {
        CancelHide();

        m_TypewriterCompletion = new UniTaskCompletionSource();

        m_CurrentSentence = sentence;
        m_TypewriterCts = new CancellationTokenSource();

        m_Label.maxVisibleCharacters = 0;
        await TypewriterAsync(m_CurrentSentence.GetLocalizedString(), m_TypewriterCts.Token);
        await UniTask.Yield(PlayerLoopTiming.Update);
    }

    public void SkipSentence()
    {
        if (m_IsComplete)
            return;

        CancelTypewriter();

        m_Label.maxVisibleCharacters = int.MaxValue;
        m_IsComplete = true;
    }

    public void Hide()
    {
        CancelTypewriter();
        CancelHide();

        m_HideCts = new CancellationTokenSource();
        HideAsync(m_HideCts.Token).Forget();
    }

    private void OnDestroy()
    {
        CancelTypewriter();
        CancelHide();
    }

    private async UniTask TypewriterAsync(string fullText, CancellationToken ct)
    {
        m_IsComplete = false;

        m_Label.text = fullText;
        m_Label.color = new Color(m_Label.color.r, m_Label.color.g, m_Label.color.b, 0f);
        m_Label.ForceMeshUpdate();

        m_Label.color = new Color(m_Label.color.r, m_Label.color.g, m_Label.color.b, 1f);

        float secondsPerChar = 1f / m_CharactersPerSecond;
        float elapsed = 0f;
        int totalChars = m_Label.textInfo.characterCount;
        int visible = m_Label.maxVisibleCharacters;

        while (visible < totalChars)
        {
            if (ct.IsCancellationRequested) return;

            elapsed += Time.deltaTime;

            int target = Mathf.Min(Mathf.FloorToInt(elapsed / secondsPerChar), totalChars);

            if (target > visible)
            {
                visible = target;
                m_Label.maxVisibleCharacters = visible;
            }

            await UniTask.Yield(PlayerLoopTiming.Update, ct).SuppressCancellationThrow();

            if (ct.IsCancellationRequested) return;
        }

        m_Label.maxVisibleCharacters = totalChars;
        m_IsComplete = true;
    }

    private async UniTaskVoid HideAsync(CancellationToken ct)
    {
        Tween fadeTween = m_Label.DOFade(0f, m_HideDuration).SetEase(m_HideEase);

        await fadeTween.AsyncWaitForCompletion().AsUniTask().AttachExternalCancellation(ct).SuppressCancellationThrow();

        if (ct.IsCancellationRequested)
        {
            fadeTween.Kill();
            return;
        }

        m_Label.text = string.Empty;
        m_Label.alpha = 1f;
    }

    private void CancelTypewriter()
    {
        m_TypewriterCts?.Cancel();
        m_TypewriterCts?.Dispose();
        m_TypewriterCts = null;
    }

    private void CancelHide()
    {
        m_HideCts?.Cancel();
        m_HideCts?.Dispose();
        m_HideCts = null;
        m_Label.DOKill();
        m_Label.alpha = 1f;
    }
}