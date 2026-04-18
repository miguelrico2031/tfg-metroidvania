// Editor/FactionMatrixEditor.cs
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FactionsData))]
public class FactionMatrixEditor : Editor
{
    public override void OnInspectorGUI()
    {
        FactionsData matrix = (FactionsData)target;
        string[] names = System.Enum.GetNames(typeof(Faction));
        int count = matrix.FactionCount;

        EditorGUILayout.LabelField("Faction Matrix:", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(140);
        for (int col = 0; col < count; col++)
            GUILayout.Label(names[col], GUILayout.Width(60));
        EditorGUILayout.EndHorizontal();

        EditorGUI.BeginChangeCheck();
        var style = new GUIStyle(GUI.skin.label);
        style.richText = true;
        for (int row = 0; row < count; row++)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"<b>{names[row]}</b> is hostile to:", style, GUILayout.Width(140));
            for (int col = 0; col < count; col++)
            {
                bool current = matrix.GetRelation(row, col);
                bool next = EditorGUILayout.Toggle(current, GUILayout.Width(60));
                if (next != current)
                    matrix.SetRelation(row, col, next);
            }
            EditorGUILayout.EndHorizontal();
        }

        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(target);
    }
}