using UnityEngine;

[RequireComponent(typeof(HealComponent))]
public class PlayerSpawnComponent : MonoBehaviour
{
    [SerializeField] private PlayerSpawner m_Spawner;

    //Called on Start because it uses data from Checkpoints which is initialized on their Awake.
    private void Start()
    {
        m_Spawner.SpawnPlayer(transform);
    }
}