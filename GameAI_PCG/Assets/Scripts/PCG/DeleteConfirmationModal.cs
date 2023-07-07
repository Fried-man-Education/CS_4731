#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

public class DeleteConfirmationModal : EditorWindow
{

    //[MenuItem("Example/Randomize Children In Selection")]
    //static void RandomizeWindow()
    //{
    //    RandomizeInSelection window = ScriptableObject.CreateInstance(typeof(RandomizeInSelection)) as RandomizeInSelection;
    //    window.ShowModalUtility();
    //}

    public delegate void DeleteAction();

    public DeleteAction deleteAction;

    void OnGUI()
    {

        GUIStyle style = new GUIStyle(EditorStyles.textArea);
        style.wordWrap = true;

        EditorGUILayout.TextArea("Really Delete? This will make PERMANMENT deletions from both the scene and the associated scriptableObject! These changes cannot be reverted/undone! You DO NOT need to delete all nodes to load a different ScriptableObject. Just select Load after setting a new SO.", style); 

        if(GUILayout.Button("Yes"))
        {
            if (deleteAction != null)
            {
                deleteAction();
                Close();
            }
            else
                Debug.LogError("No delete action assigned!");
        }

        if (GUILayout.Button("No"))
            Close();
    }

    void OnInspectorUpdate()
    {
        Repaint();
    }

}

#endif