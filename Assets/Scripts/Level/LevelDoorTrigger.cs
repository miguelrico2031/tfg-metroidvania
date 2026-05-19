using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LevelDoorTrigger : MonoBehaviour
{
    [SerializeField] private LevelLoader m_LevelLoader;
    [SerializeField] private string m_TargetLevelName;
    [SerializeField] private int m_TargetLevelEntrypointID;

    bool m_Triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!m_Triggered)
        {
            m_Triggered = true;
            Entrypoint.SetActiveEntrypoint(m_TargetLevelEntrypointID);
            _ = m_LevelLoader.LoadLevel(m_TargetLevelName, LevelLoader.Reason.DoorTriggered);
        }
    }
}
