using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [SerializeField] private int m_ID;
    [SerializeField] private DataReference<IPersistence> m_Persistence;

    private Collider2D m_Collider;

    private readonly static Dictionary<int, Checkpoint> s_UniqueIDs = new();
    private static IPersistence s_Persistence;

    public static bool TryGetActiveCheckpoint(out Checkpoint checkpoint)
    {
        checkpoint = null;
        if(s_Persistence.TryGetEntry(PersistentData.ActiveCheckpoint, out string idStr) &&
            int.TryParse(idStr, out int ID))
        {
            checkpoint = s_UniqueIDs[ID];
            return true;
        }
        return false;
    }

    private void Awake()
    {
        Register();

        m_Collider = GetComponent<Collider2D>();
        m_Collider.isTrigger = true;
    }

    //GameObject must be set to layer Checkpoint to only check collisions with Player
    private void OnTriggerEnter2D(Collider2D other) 
    {
        m_Persistence.Value.SetEntry(PersistentData.ActiveCheckpoint, m_ID.ToString());
        m_Persistence.Value.Save();
    }

    private void Register()
    {
        Assert.IsTrue(m_ID > 0, "Invalid Checkpoint ID.");
        bool unique = s_UniqueIDs.TryAdd(m_ID, this) || s_UniqueIDs[m_ID] == this;
        Assert.IsTrue(unique, $"Checkpoint ID is not unique: {m_ID}");
        s_Persistence ??= m_Persistence.Value;
    }
}

//public class 