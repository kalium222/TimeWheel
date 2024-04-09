using System;
using UnityEngine;
using UnityEditor;
using Timer;

public class TimerManagerWindow : EditorWindow
{
    bool groupEnabled;
    public int ms, s, min, hour, day;
    public int i_ms, i_s, i_min, i_hour, i_day;
    public int times;

    private bool m_foldAddTimer = true;

    [MenuItem("Window/MyWindow/TimerManager")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(TimerManagerWindow));
    }

    private void OnGUI()
    {
        GUILayout.Label ("Timer Manager", EditorStyles.boldLabel);

        Timer.TimerManager instance = Timer.TimerManager.s_instance;
        if ( instance==null ) 
        {
            GUILayout.Label("Enter Play mode to instantiate the Timer Manager.");
            return;
        }

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
            GUILayout.Label("expire: ");
            ms = EditorGUILayout.IntField("1/10s", ms);
            s = EditorGUILayout.IntField("s", s);
            min = EditorGUILayout.IntField("min", min);
            hour = EditorGUILayout.IntField("hour", hour);
            day = EditorGUILayout.IntField("day", day);
            TimeSpan expire = new(day, hour, min, s, ms);

            GUILayout.Label("interval: ");
            i_ms = EditorGUILayout.IntField("1/10s", i_ms);
            i_s = EditorGUILayout.IntField("s", i_s);
            i_min = EditorGUILayout.IntField("min", i_min);
            i_hour = EditorGUILayout.IntField("hour", i_hour);
            i_day = EditorGUILayout.IntField("day", i_day);
            TimeSpan interval = new(i_day, i_hour, i_min, i_s, i_ms);

            GUILayout.Label("times: ");
            times = EditorGUILayout.IntField("times", times);
            
            if ( GUILayout.Button("Add"))
            {
                instance.AddTimer(expire, interval, (uint)times, ()=>{
                    Debug.Log(DateTime.Now.Millisecond);
                });
                Debug.Log("Added a Timer!");
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.LabelField("Timer Count: ", instance.Count.ToString());
        
        groupEnabled = EditorGUILayout.BeginToggleGroup ("Optional Settings", groupEnabled);
        GUILayout.Label("toggle group!");
        EditorGUILayout.EndToggleGroup ();

        EditorGUILayout.LabelField("pressure test: ");
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("100,000 and execute"))
        {
            // TODO: 

        }
        EditorGUILayout.Space(5);
        if (GUILayout.Button("1000,000 add"))
        {
            // TODO: 
        }
        EditorGUILayout.EndHorizontal();

        foreach (TimeWheel tw in instance.TimeWheelArray)
        {
            GUILayout.Label("TimerList: ");
            foreach (int i in tw.GetDistri())
            {
                GUILayout.Label(i.ToString());
            }
        }
    }
}
