using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Entrypoint : MonoBehaviour
{
    [SerializeField] private int m_ID;

    private readonly static Dictionary<int, Entrypoint> s_UniqueIDs = new();
    private static int? s_ActiveEntryPointID = null;

    public static bool TryGetActiveEntrypoint(out Entrypoint entrypoint)
    {
        entrypoint = null;
        if (s_ActiveEntryPointID is not null && 
            s_UniqueIDs.TryGetValue(s_ActiveEntryPointID.Value, out entrypoint) &&
            entrypoint != null)
        {
            return true;
        }
        return false;
    }

    public static void SetActiveEntrypoint(int ID)
    {
        Assert.IsTrue(s_ActiveEntryPointID is null, "Active Entrypoint reset before cleared.");
        s_ActiveEntryPointID = ID;
    }

    public static void ClearActiveEntrypoint() => s_ActiveEntryPointID = null;

    private void Awake()
    {
        s_UniqueIDs[m_ID] = this;
    }
}