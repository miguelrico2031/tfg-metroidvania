using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/DialogueData")]
public class DialogueData : ScriptableObject
{
    [field: SerializeField] public DialogueSentence[] Sentences { get; private set; }
}