using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using UnityEngine;

namespace Timer
{
    public class TimeWheel
    {
        // private
        private readonly ulong m_tickMs;
        private readonly int m_wheelSize;
        private int m_current;
        private readonly TimerList[] m_bucketArray;

        public ulong TickMs
        {
            get { return m_tickMs; }
        }

        public int WheelSize
        {
            get { return m_wheelSize; }
        }

        public ulong MaxTime
        {
            get { return m_tickMs*(ulong)m_wheelSize; }
        }

        public int Count 
        {
            get
            {
                int result = 0;
                foreach ( TimerList l in m_bucketArray )
                {
                    result += l.Count;
                }
                return result;
            }
        }

        // public method
        public TimeWheel(ulong tickMs, int wheelSize)
        {
            m_current = 0;
            m_tickMs = tickMs;
            m_wheelSize = wheelSize;
            m_bucketArray = new TimerList[wheelSize];
            for (int i=0; i<m_wheelSize; i++)
                m_bucketArray[i] = new TimerList();
        }

        // 将move和dotask交给HierachicalTimeWheel完成
        // Tick仅仅改变current
        // 进位则返回true
        public bool Tick()
        {
            m_current++;
            bool carry =  m_current==m_wheelSize ;
            m_current %= m_wheelSize;
            return carry;
        }

        // 该timewheel上，现在走过的时间
        public ulong GetCurrentTime()
        {
            return ((ulong)m_current) * m_tickMs;
        }
        
        public TimerList GetCurrentTimerList()
        {
            return m_bucketArray[m_current];
        }

        public List<int> GetDistri()
        {
            List<int> res = new();
            foreach ( TimerList l in m_bucketArray )
            {
                res.Add(l.Count);
            }
            return res;
        }

        // 不判断条件
        public void AddTimer(Timer timer)
        {
            m_bucketArray[(int)(timer.expire/m_tickMs)%m_wheelSize].Add(timer);
        }

        // 从timewheel中移除timer
        public static void DetachTimer(Timer timer)
        {
            TimerList.Detach(timer);
        }

        public void ClearAll()
        {
            m_current = 0;
            foreach ( TimerList l in m_bucketArray )
            {
                l.Clear();
            }
        }
    }

    [AddComponentMenu("Timer/TimerManager")]
    public class TimerManager : MonoBehaviour 
    {
        // private field
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public static TimerManager? s_instance;
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        // 由小到大
        private List<TimeWheel> m_timeWheelArray;
        private ConcurrentDictionary<uint, Timer> m_timerTable;
        private uint m_maxId = 0;
        private bool m_stop = true;
        private float m_deltaTime = 0;

        private void Awake()
        {
            if ( s_instance==null )
            {
                s_instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            m_timeWheelArray = new List<TimeWheel>
            {
                // ms, s, min, hour, day
                new(1, 10),
                new(1 * 10, 60),
                new(1 * 10 * 60, 60),
                new(1 * 10 * 60 * 60, 24),
                new(1 * 10 * 60 * 60 * 24, 50)
            };
            m_timerTable = new ConcurrentDictionary<uint, Timer>();
        }

        private void Update()
        {
            if ( !m_stop ) 
                m_deltaTime += Time.deltaTime;
            if ( m_deltaTime>=0.1 )
            {
                m_deltaTime = 0;
                Tick();
            }
        }

        // 完成所有在最小timewheel里的Timer
        // 将其他timewheels里当前的timer向下移
        // 重复任务重新放置
        private void Tick()
        {
            bool carry = true;
            foreach ( TimeWheel tw in m_timeWheelArray )
            {
                if ( !carry )
                    break;
                carry = tw.Tick();
                if ( tw == m_timeWheelArray[0] )
                {
                    foreach ( Timer t in tw.GetCurrentTimerList() )
                    {
                        t.DoTask();
                        if ( t.times<=1 )
                        {
                            this.RemoveTimer(t);
                        }
                        else
                        {
                            this.ModifyTimer(t, t.interval+this.GetCurrentTime(), 
                                    t.interval, t.times-1);
                        }
                    }
                }
                else // 向下移动    
                {
                    foreach ( Timer t in tw.GetCurrentTimerList() )
                        this.RefreshTimer(t);
                }
            }
        }

        private uint GetAvaliableId()
        {
            if ( m_maxId == uint.MaxValue )
                m_maxId = 0;
            while ( m_timerTable.ContainsKey(m_maxId) )
                m_maxId++;
            return m_maxId;
        }

        public List<TimeWheel> TimeWheelArray
        {
            get { return m_timeWheelArray; }
        }

        // return true if started succesfully
        // return false if instance has already
        // been launched
        public bool StartRunning()
        {
            if ( m_stop )
            {
                m_stop = false;
                return true;
            }
            else 
            {
                return false;
            }
        }

        public bool IsRunning
        {
            get { return !m_stop; }
        }

        public Timer GetTimer(uint id)
        {
            return m_timerTable[id];
        }

        public List<int> GetDistri()
        {
            List<int> result = new(m_timeWheelArray.Count);
            for ( int i=0; i<result.Capacity; i++ )
            {
                result.Add(m_timeWheelArray[i].Count);
            }
            return result;
        }

        public int Count
        {
            get
            {
                int res = 0;
                foreach ( int i in GetDistri() )
                {
                    res += i;
                }
                return res;
            }
        }

        public ulong GetCurrentTime()
        {
            ulong result = 0;
            foreach ( TimeWheel timewheel in m_timeWheelArray )
                result += timewheel.GetCurrentTime();
            return result;
        }

        public ulong GetMaxTime()
        {
            return m_timeWheelArray.Last().MaxTime;
        }

        // 把timer加入Dictionary
        // 根据时间把timer加入合适的timewheel
        public uint AddTimer(ulong expire, ulong interval, uint times, Action task)
        {
            Timer newTimer = new(GetAvaliableId(), expire, interval, times, task);
            this.AddTimer(newTimer);
            return newTimer.Id;
        }

        public void AddTimer(Timer timer)
        {
            timer.Id = GetAvaliableId();
            m_timerTable.TryAdd(timer.Id, timer);
            ulong idx = timer.expire - this.GetCurrentTime();
            if ( (long)idx <= 0 )
                m_timeWheelArray.First().AddTimer(timer);
            else if ( idx >= this.GetMaxTime() )
                m_timeWheelArray.Last().AddTimer(timer);
            else
            {
                foreach ( TimeWheel tw in m_timeWheelArray )
                {
                    if ( idx <= tw.MaxTime )
                    {
                        tw.AddTimer(timer);
                        break;
                    }
                }
            }
        }

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public Timer? RemoveTimer(Timer timer)
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        {
            m_timerTable.TryRemove(timer.Id, out Timer res);
            TimerList.Detach(timer);
            return res;
        }

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public Timer? RemoveTimer(uint id)
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        {
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
            Timer? timer = m_timerTable[id];
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
            m_timerTable.Remove(id, out timer);
            TimerList.Detach(timer);
            return timer;
        }

        public int RefreshTimer(Timer timer)
        {
            this.RemoveTimer(timer);
            this.AddTimer(timer);
            return 0;
        }

        public int ModifyTimer(Timer timer, ulong postpone, ulong interval, uint times)
        {
            timer.expire = postpone;
            timer.interval = interval;
            timer.times = times;
            this.RefreshTimer(timer);
            return 0;
        }

        public int ModifyTimer(uint id, ulong postpone, ulong interval, uint times)
        {
            Timer modified = GetTimer(id);
            this.ModifyTimer(modified, postpone, interval, times);
            return 0;
        }

        public void Reset()
        {
            m_stop = true;
            m_timerTable.Clear();
            foreach ( TimeWheel tw in m_timeWheelArray )
                tw.ClearAll();
        }

    }

}