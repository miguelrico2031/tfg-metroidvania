using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugCheats : MonoBehaviour
{
    [SerializeField] private DataReference<IPersistence> m_Persistence;

    private readonly StringBuilder m_CurrentCommand = new();
    private bool m_InvulnerableCheatSet = false;
    private AttackTargetComponent m_PlayerAttackTarget;
    private Hitbox[] m_PlayerHitboxes;
    private AttackData[] m_DefaultAttacksData;

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

        m_PlayerHitboxes = player.GetComponentsInChildren<Hitbox>(includeInactive: true);
        m_DefaultAttacksData = new AttackData[m_PlayerHitboxes.Length];
        for (int i = 0; i < m_PlayerHitboxes.Length; i++)
        {
            m_DefaultAttacksData[i] = m_PlayerHitboxes[i].AttackData;
        }
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
                Debug.Log("- console1");
                Debug.Log("- console0");
                Debug.Log("- clearsave");
                Debug.Log("- invul1");
                Debug.Log("- invul0");
                break;

            case "console1":
                Debug.developerConsoleEnabled = true;
                Debug.Log("[CHEAT] console1");
                break;

            case "console0":
                Debug.developerConsoleEnabled = false;
                Debug.Log("[CHEAT] console0");
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

            case "onehit1":
                foreach(var hitbox in m_PlayerHitboxes)
                {
                    AttackData data = hitbox.AttackData;
                    data.Damage = 999;
                    hitbox.AttackData = data;
                }
                Debug.Log("[CHEAT] onehit1");
                break;

            case "onehit0":
                for (int i = 0; i < m_PlayerHitboxes.Length; i++)
                {
                    m_PlayerHitboxes[i].AttackData = m_DefaultAttacksData[i];
                }
                Debug.Log("[CHEAT] onehit0");
                break;
        }
    }
#endif
}