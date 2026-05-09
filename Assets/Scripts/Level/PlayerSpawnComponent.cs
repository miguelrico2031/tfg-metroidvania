using UnityEngine;

[RequireComponent(typeof(HealComponent))]
public class PlayerSpawnComponent : MonoBehaviour
{
    //Called on Start because it uses data from Checkpoints which is initialized on their Awake.
    private void Start()
    {
        Vector3 spawnPosition = transform.position;
        if(Entrypoint.TryGetActiveEntrypoint(out var entrypoint))
        {
            Entrypoint.ClearActiveEntrypoint();
            spawnPosition = entrypoint.transform.position;
        }
        else if (Checkpoint.GetActiveLevelCheckpoint(out var checkpoint))
        {
            spawnPosition = checkpoint.transform.position;
        }

        transform.position = spawnPosition;
    }
}