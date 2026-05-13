using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Localization;

[RequireComponent(typeof(Interactable))]
public class NPCDialogue : MonoBehaviour
{
    [SerializeField] private float m_MaxWaitingTime;
    [SerializeField] private DialogueData m_DialogueData;
    [SerializeField] private DialogueDisplayer m_DialogueDisplayer;

    private DialogueState m_CurrentState = DialogueState.NotTalking;
    private int m_CurrentDialogueIndex = -1;
    private Interactable m_Interactable;

    private CancellationTokenSource m_WaitCts;

    private void Awake()
    {
        m_Interactable = GetComponent<Interactable>();
    }

    private void OnEnable()
    {
        m_Interactable.OnInteract += OnInteract;
    }

    private void OnDisable()
    {
        m_Interactable.OnInteract -= OnInteract;
    }

    private void OnInteract()
    {
        switch (m_CurrentState)
        {
            case DialogueState.NotTalking:
                StartDialogue();
                break;
            case DialogueState.Talking:
                SkipSentence();
                break;
            case DialogueState.Waiting:
                AdvanceDialogue();
                break;
        }
    }

    private void StartDialogue()
    {
        CancelWait();
        m_CurrentDialogueIndex = 0;
        DisplayDialogue().Forget();
    }

    private void SkipSentence()
    {
        Debug.Log($"[Frame {Time.frameCount}] Skip sentence start.");
        m_DialogueDisplayer.SkipSentence();
        Debug.Log($"[Frame {Time.frameCount}] Skip sentence finished.");
    }

    private void AdvanceDialogue()
    {
        CancelWait();
        if (++m_CurrentDialogueIndex < m_DialogueData.Sentences.Length)
        {
            DisplayDialogue().Forget();
        }
        else
        {
            EndDialogue();
        }
    }

    private async UniTask DisplayDialogue()
    {
        m_CurrentState = DialogueState.Talking;
        Debug.Log($"[Frame {Time.frameCount}] Displaying dialogue {m_CurrentDialogueIndex}");
        await m_DialogueDisplayer.DisplaySentenceAsync(m_DialogueData.Sentences[m_CurrentDialogueIndex].String);
        
        Debug.Log($"[Frame {Time.frameCount}] Displayed dialogue {m_CurrentDialogueIndex} finished.");

        m_WaitCts = new CancellationTokenSource();
        Wait(m_WaitCts.Token).Forget();
    }

    private async UniTask Wait(CancellationToken ct)
    {
        m_CurrentState = DialogueState.Waiting;
        Debug.Log($"[Frame {Time.frameCount}] Waiting.");
        await UniTask.Delay(TimeSpan.FromSeconds(m_MaxWaitingTime), cancellationToken: ct).SuppressCancellationThrow();
        if (!ct.IsCancellationRequested)
        {
            EndDialogue();
        }
    }

    private void CancelWait()
    {
        m_WaitCts?.Cancel();
        m_WaitCts?.Dispose();
        m_WaitCts = null;
    }

    private void EndDialogue()
    {
        Debug.Log($"[Frame {Time.frameCount}] Dialogue ended.");
        m_CurrentDialogueIndex = -1;
        m_CurrentState = DialogueState.NotTalking;
        m_DialogueDisplayer.Hide();
    }

    public enum DialogueState
    {
        NotTalking,
        Talking,
        Waiting,
    }
}