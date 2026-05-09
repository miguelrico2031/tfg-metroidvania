using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private DataReference<IPersistence> m_Persistence;

    private static Checkpoint s_LevelCheckpoint;

    public static bool GetActiveLevelCheckpoint(out Checkpoint checkpoint)
    {
        checkpoint = s_LevelCheckpoint;
        return checkpoint != null &&
            checkpoint.m_Persistence.Value.TryGetEntry(PersistentData.ActiveCheckpointLevel, out string level) &&
            level == SceneManager.GetActiveScene().name;
    }

    private void Awake()
    {
        s_LevelCheckpoint = this;
    }

    //GameObject must be set to layer PlayerTrigger to only check collisions with Player
    private void OnTriggerEnter2D(Collider2D other) 
    {
        m_Persistence.Value.SetEntry(PersistentData.ActiveCheckpointLevel, SceneManager.GetActiveScene().name);
        m_Persistence.Value.Save();
    }
}