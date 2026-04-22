using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [SerializeField] private int m_ID;
    [SerializeField] private bool m_IsDefault;
    [SerializeField] private DataReference<IPersistence> m_Persistence;

    private Collider2D m_Collider;

    private readonly static Dictionary<int, Checkpoint> s_UniqueIDs = new();
    private static Checkpoint s_Default;

    public static Checkpoint GetActiveCheckpoint()
    {
        if(s_Default.m_Persistence.Value.Load(PersistentData.ActiveCheckpoint, out int ID))
        {
            return s_UniqueIDs[ID];
        }
        return s_Default;
    }

    private void Awake()
    {
        Register();

        m_Collider = GetComponent<Collider2D>();
        m_Collider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        //GameObject must be set to layer Checkpoint to only check collisions with Player
        m_Persistence.Value.Save(PersistentData.ActiveCheckpoint, m_ID);
    }

    private void Register()
    {
        Assert.IsTrue(m_ID > 0, "Invalid Checkpoint ID.");
        bool unique = s_UniqueIDs.TryAdd(m_ID, this) || s_UniqueIDs[m_ID] == this;
        Assert.IsTrue(unique, $"Checkpoint ID is not unique: {m_ID}");
        if(m_IsDefault)
        {
            Assert.IsNull(s_Default, "Default checkpoint assigned multiple times.");
            s_Default = this;
        }
    }
}

//public class 