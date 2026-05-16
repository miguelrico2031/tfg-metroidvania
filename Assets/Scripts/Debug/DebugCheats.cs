using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugCheats : MonoBehaviour
{
    [SerializeField] private DataReference<IPersistence> m_Persistence;

    private readonly StringBuilder m_CurrentCommand = new();
    private bool m_InvulnerableCheatSet = false;
    private AttackTargetComponent m_PlayerAttackTarget;

#if CHEATS_ENABLED
    private void OnEnable()
    {
        Keyboard.current.onTextInput += OnTextInput;
    }
    private void OnDisable()
    {
        Keyboard.current.onTextInput -= OnTextInput;
    }

    private void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        m_PlayerAttackTarget = player.GetComponent<AttackTargetComponent>();
    }

    private void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame || m_CurrentCommand.Length > 20)
        {
            m_CurrentCommand.Clear();
        }
    }

    private void OnTextInput(char c)
    {
        m_CurrentCommand.Append(c);
        CheckCommand();
    }

    private void CheckCommand()
    {
        switch (m_CurrentCommand.ToString().ToLower())
        {
            case "help":
                Debug.Log("[CHEAT] help: list of all cheats:");
                Debug.Log("- clearsave");
                Debug.Log("- invul1");
                Debug.Log("- invul0");
                break;

            case "clearsave":
                m_Persistence.Value.ClearAllEntries();
                m_Persistence.Value.Save();
                Debug.Log("[CHEAT] clearsave");
                break;

            case "invul1":
                if (!m_InvulnerableCheatSet)
                {
                    m_InvulnerableCheatSet = true;
                    m_PlayerAttackTarget.AddInvulnerability();
                    Debug.Log("[CHEAT] invul1");
                }
                break;

            case "invul0":
                if (m_InvulnerableCheatSet)
                {
                    m_InvulnerableCheatSet = false;
                    m_PlayerAttackTarget.RemoveInvulnerability();
                    Debug.Log("[CHEAT] invul0");
                }
                break;
        }
    }
#endif
}