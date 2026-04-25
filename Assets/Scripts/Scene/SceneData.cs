using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="ScriptableObjects/SceneData")]
public class SceneData : ScriptableObject
{
    [field: SerializeField] public string SceneName { get; private set; }

}
