using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TimerManagerWindow : EditorWindow
{
    string myString = "Hello World";
    bool groupEnabled;
    bool myBool = true;
    float myFloat = 1.23f;

    private bool m_foldAddTimer = true;

    [MenuItem("Window/TimerManager")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(TimerManagerWindow));
    }

    private ulong int2time(int m, int s, int min, int h, int d)
    {
        ulong result = 0;
    }

    private void OnGUI()
    {
        GUILayout.Label ("Timer Manager", EditorStyles.boldLabel);

        myString = EditorGUILayout.TextField ("Text Field", myString);
        if (GUILayout.Button("Start TimerManager"))
        {
            TimerManager.TimerManager.s_instance.StartRunning();
            Debug.Log("Started TimerManager!");
        }
        
        m_foldAddTimer = EditorGUILayout.BeginFoldoutHeaderGroup(m_foldAddTimer, "Add Timer");
        if ( m_foldAddTimer )
        {
            GUILayout.Button("Add");
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        
        groupEnabled = EditorGUILayout.BeginToggleGroup ("Optional Settings", groupEnabled);
            myBool = EditorGUILayout.Toggle ("Toggle", myBool);
            myFloat = EditorGUILayout.Slider ("Slider", myFloat, -3, 3);
        EditorGUILayout.EndToggleGroup ();
    }
}
