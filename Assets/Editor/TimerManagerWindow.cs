using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Timer;

namespace TimerManagerWindow
{
    public class TimerManagerWindow : EditorWindow
    {
        // private field
        // for displaying the fields correctly
        private int m_id;
        private int m_ms, m_s, m_min, m_hour, m_day;
        private int m_interval_ms, m_interval_s, m_interval_min;
        private int m_interval_hour, m_interval_day;
        private int m_times;
        private int m_a, m_b;
        private bool m_foldAddTimer = false;
        private bool m_foldModifyTimer = false;
        private readonly Queue<Timer.Timer> m_addedTimerDisplayQueue = new();
        private const int m_displayTimerSize = 15;
        private const int k_pressExecute = 10_0000;
        private const int k_pressAdd = 100_0000;

        public Action<int, int> callback = (int a, int b) => {
            Debug.Log("ms: " + DateTime.Now.Millisecond + ", Timer a: " + a + ", b: " + b);
        };

        private void MaintainQueue(Timer.Timer timer)
        {
            if (m_addedTimerDisplayQueue.Contains(timer))
                return;
            m_addedTimerDisplayQueue.Enqueue(timer);
            if ( m_addedTimerDisplayQueue.Count > m_displayTimerSize )
                m_addedTimerDisplayQueue.Dequeue();
        }

        private TimeSpan GetRandomTimeSpan()
        {
            System.Random rd = new();
            return new TimeSpan(0, 0, 0, rd.Next(0, 10), rd.Next(0, 1000));
        }

        private uint GetRandomTimes()
        {
            System.Random rd = new();
            return (uint)rd.Next(1, 5);
        }

        private void AddRandomTimer(int index)
        {
            TimerManager instance = TimerManager.s_instance;
            TimeSpan expire = GetRandomTimeSpan();
            TimeSpan interval = GetRandomTimeSpan();
            uint times = (uint)GetRandomTimes();
            instance.AddTimer(expire, interval, times, index, index);
        }

        [MenuItem("Window/MyWindow/TimerManager")]
        public static void ShowWindow()
        {
            GetWindow(typeof(TimerManagerWindow));
        }

        private void OnGUI()
        {
            GUILayout.Label ("Timer Manager", EditorStyles.boldLabel);
            GUILayout.Space(10);

            TimerManager instance = TimerManager.s_instance;
            if ( instance==null ) 
            {
                GUILayout.Label("Enter Play mode to instantiate the Timer Manager.");
                return;
            }

            EditorGUILayout.LabelField("isRunning:", instance.IsRunning.ToString());
            GUILayout.Space(10);

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
            GUILayout.Space(10);

            m_foldAddTimer = EditorGUILayout.BeginFoldoutHeaderGroup(m_foldAddTimer, "Add Timer");
            if ( m_foldAddTimer )
            {
                // close the Modify folder
                m_foldModifyTimer = false;
                GUILayout.Label("expire: ", EditorStyles.boldLabel);
                m_ms = EditorGUILayout.IntField("ms", m_ms);
                m_s = EditorGUILayout.IntField("s", m_s);
                m_min = EditorGUILayout.IntField("min", m_min);
                m_hour = EditorGUILayout.IntField("hour", m_hour);
                m_day = EditorGUILayout.IntField("day", m_day);
                TimeSpan expire = new(m_day, m_hour, m_min, m_s, m_ms);

                GUILayout.Label("interval: ", EditorStyles.boldLabel);
                m_interval_ms = EditorGUILayout.IntField("1/10s", m_interval_ms);
                m_interval_s = EditorGUILayout.IntField("s", m_interval_s);
                m_interval_min = EditorGUILayout.IntField("min", m_interval_min);
                m_interval_hour = EditorGUILayout.IntField("hour", m_interval_hour);
                m_interval_day = EditorGUILayout.IntField("day", m_interval_day);
                TimeSpan interval = new(m_interval_day, m_interval_hour, m_interval_min, m_interval_s, m_interval_ms);

                GUILayout.Label("times: ", EditorStyles.boldLabel);
                m_times = EditorGUILayout.IntField("times", m_times);

                GUILayout.Label("param: ", EditorStyles.boldLabel);
                m_a = EditorGUILayout.IntField("param1: ", m_a);
                m_b = EditorGUILayout.IntField("param2: ", m_b);

            
                if ( GUILayout.Button("Add"))
                {
                    uint id = instance.AddTimer(expire, interval, (uint)m_times, m_a, m_b, callback);
                    Debug.Log("Added a Timer!");
                    MaintainQueue(instance.GetTimer(id));
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            GUILayout.Space(10);

            m_foldModifyTimer = EditorGUILayout.BeginFoldoutHeaderGroup(m_foldModifyTimer, "Modify Timer");
            if ( m_foldModifyTimer )
            {
                // close the Add folder
                m_foldAddTimer = false;
                GUILayout.Label("Id", EditorStyles.boldLabel);
                m_id = EditorGUILayout.IntField("id: ", m_id);

                GUILayout.Label("expire: ", EditorStyles.boldLabel);
                m_ms = EditorGUILayout.IntField("ms", m_ms);
                m_s = EditorGUILayout.IntField("s", m_s);
                m_min = EditorGUILayout.IntField("min", m_min);
                m_hour = EditorGUILayout.IntField("hour", m_hour);
                m_day = EditorGUILayout.IntField("day", m_day);
                TimeSpan expire = new(m_day, m_hour, m_min, m_s, m_ms);

                GUILayout.Label("interval: ", EditorStyles.boldLabel);
                m_interval_ms = EditorGUILayout.IntField("1/10s", m_interval_ms);
                m_interval_s = EditorGUILayout.IntField("s", m_interval_s);
                m_interval_min = EditorGUILayout.IntField("min", m_interval_min);
                m_interval_hour = EditorGUILayout.IntField("hour", m_interval_hour);
                m_interval_day = EditorGUILayout.IntField("day", m_interval_day);
                TimeSpan interval = new(m_interval_day, m_interval_hour, 
                        m_interval_min, m_interval_s, m_interval_ms);

                GUILayout.Label("times: ", EditorStyles.boldLabel);
                m_times = EditorGUILayout.IntField("times", m_times);

                GUILayout.Label("param: ", EditorStyles.boldLabel);
                m_a = EditorGUILayout.IntField("param1: ", m_a);
                m_b = EditorGUILayout.IntField("param2: ", m_b);
            
                EditorGUILayout.BeginHorizontal();
                if ( GUILayout.Button("Modify"))
                {
                    if (instance.ModifyTimer(id:(uint)m_id, expire, interval,
                                                    (uint)m_times, m_a, m_b))
                        Debug.Log("Modified a Timer by id: " + m_id.ToString() + "!");
                    else
                        Debug.Log("No such key!");
                }
                if ( GUILayout.Button("Remove by id"))
                {
                    if (instance.RemoveTimer((uint)m_id))
                        Debug.Log("Removed a Timer by id: " + m_id.ToString() + "!");
                    else
                        Debug.Log("No such key!");
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            GUILayout.Space(10);

            GUILayout.Label("Latest timers: ", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical();
            foreach ( Timer.Timer t in m_addedTimerDisplayQueue )
            {
                string message = "Id: " + t.Id + ", expire: " + t.expire.ToString();
                if ( instance.GetTimer(t.Id)==null )
                    message += ", deleted!";
                GUILayout.Label(message);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.LabelField("Timer Count: ", instance.Count.ToString());
            GUILayout.Space(10);

            EditorGUILayout.LabelField("pressure test: ", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("100,000 and execute"))
            {
                for (int i=0; i<k_pressExecute; i++)
                {
                    AddRandomTimer(i);
                }
            }
            EditorGUILayout.Space(5);
            if (GUILayout.Button("1000,000 add"))
            {
                for (int i=0; i<k_pressAdd; i++)
                {
                    // AddRandomTimer(i);
                    instance.AddTimer(new(), new(), new(), new(), new());
                }
            }
            EditorGUILayout.EndHorizontal();

            #if TIMER_MANAGER_DEBUG 
            foreach (var tw in instance.TimeWheelArray)
            {
                if (tw == instance.TimeWheelArray[0])
                    continue;
                foreach (var l in tw.BucketArray)
                {
                    GUILayout.Label(l.Count.ToString());
                }
            }
            #endif

        }
    }
}
