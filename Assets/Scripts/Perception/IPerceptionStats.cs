using UnityEngine;

public interface IPerceptionStats
{
    public Vector2 EdgeCheckOffset { get; }
    public float EdgeCheckDepth { get; }

    public Vector2 GroundCheckSize { get; }
    public float GroundCheckOffset { get; }
    public LayerMask GroundLayers { get; }
    public float CoyoteTime { get; }

    public Vector2 ObstacleCheckSize { get; }
    public float ObstacleCheckOffset { get; }
    public LayerMask ObstacleLayers { get; }
}