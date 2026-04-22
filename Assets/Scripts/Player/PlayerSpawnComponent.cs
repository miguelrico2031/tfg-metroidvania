using UnityEngine;

public class PlayerSpawnComponent : MonoBehaviour
{
    [SerializeField] private float m_CheckpointDistanceTolerance;
    public void Start() //This script has negative execution order. So position dependant logic should be done in Start().
    {
        MoveToActiveCheckpoint();
    }

    private void MoveToActiveCheckpoint()
    {
        var activeCheckpoint = Checkpoint.GetActiveCheckpoint();
        if (Vector2.Distance(activeCheckpoint.transform.position, transform.position) > m_CheckpointDistanceTolerance)
        {
            transform.position = activeCheckpoint.transform.position;
        }
    }
}