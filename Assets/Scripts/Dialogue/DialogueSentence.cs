using System;
using UnityEngine;
using UnityEngine.Localization;

[Serializable]
public class DialogueSentence
{
    [field: SerializeField] public LocalizedString String { get; private set; }
}
