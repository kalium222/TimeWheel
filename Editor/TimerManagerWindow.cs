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

    [MenuItem("Window/MyWindow/TimerManager")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(TimerManagerWindow));
    }

    private ulong int2time(int m, int s, int min, int h, int d)
    {
        ulong result = 0;
        TimerManager.TimerManager instance = TimerManager.TimerManager.s_instance;
        List<TimerManager.TimeWheel> list = instance.TimeWheelArray;
        result = (ulong)m*list[0].TickMs + (ulong)s*list[1].TickMs + (ulong)min*list[2].TickMs
            + (ulong)h*list[3].TickMs + (ulong)d*list[4].TickMs;
        return result;
    }

    private void OnGUI()
    {
        GUILayout.Label ("Timer Manager", EditorStyles.boldLabel);

        TimerManager.TimerManager instance = TimerManager.TimerManager.s_instance;
        if ( instance==null ) 
        {
            GUILayout.Label("Enter Play mode to instantiate the Timer Manager.");
            return;
        }

        myString = EditorGUILayout.TextField("Test Field", myString);
        EditorGUILayout.LabelField("isRunning:", instance.IsRunning.ToString());

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Start"))
        {
            if ( instance.StartRunning())
                Debug.Log("Started TimerManager!");
            else
                Debug.Log("Instance have already been launched!");
        }
        EditorGUILayout.Space(5);
        if (GUILayout.Button("Reset"))
        {
            instance.Reset();
            Debug.Log("Reset TimerManager!");
        }
        EditorGUILayout.EndHorizontal();

        m_foldAddTimer = EditorGUILayout.BeginFoldoutHeaderGroup(m_foldAddTimer, "Add Timer");
        if ( m_foldAddTimer )
        {
            int m = EditorGUILayout.IntField("1/10s", 0);
            int s = EditorGUILayout.IntField("s", 0);
            int min = EditorGUILayout.IntField("min", 0);
            int hour = EditorGUILayout.IntField("hour", 0);
            int day = EditorGUILayout.IntField("day", 0);
            GUILayout.Button("Add");
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        
        groupEnabled = EditorGUILayout.BeginToggleGroup ("Optional Settings", groupEnabled);
            myBool = EditorGUILayout.Toggle ("Toggle", myBool);
            myFloat = EditorGUILayout.Slider ("Slider", myFloat, -3, 3);
        EditorGUILayout.EndToggleGroup ();
    }
}
