using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "ScriptableObjects/LevelLoader")]
public class LevelLoader : ScriptableObject
{
    [field: SerializeField] public string[] Scenes { get; private set; }

    public event Action OnLevelLoadStarted;
    public event Action OnLevelLoadCompleted;

    public async void LoadLevel(string name)
    {
        OnLevelLoadStarted?.Invoke();
        await SceneManager.LoadSceneAsync(name);
        OnLevelLoadCompleted?.Invoke();
    }
}
