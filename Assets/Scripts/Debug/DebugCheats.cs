using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugCheats : MonoBehaviour
{
    [SerializeField] private DataReference<IPersistence> m_Persistence;
    private readonly StringBuilder m_CurrentCommand = new();

#if CHEATS_ENABLED
    private void OnEnable()
    {
        Keyboard.current.onTextInput += OnTextInput;
    }
    private void OnDisable()
    {
        Keyboard.current.onTextInput -= OnTextInput;
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
            break;

        case "clearsave":
            m_Persistence.Value.ClearAllEntries();
            m_Persistence.Value.Save();
            Debug.Log("[CHEAT] clearsave");
            break;
        }
    }
#endif
}