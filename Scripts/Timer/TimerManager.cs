using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using UnityEngine;

namespace Timer
{
    public class TimeWheel
    {
        // private field
        // current index of the bucket array
        private int m_current;
        private readonly TimerList[] m_bucketArray;

        // public field
        public readonly TimeSpan tickMs;
        public readonly int wheelSize;
        
        public TimeSpan MaxTimeSpan => tickMs * wheelSize;

        // public method
        public TimeWheel(TimeSpan tickMs, int wheelSize)
        {
            m_current = 0;
            this.tickMs = tickMs;
            this.wheelSize = wheelSize;
            m_bucketArray = new TimerList[wheelSize];
            for (int i=0; i< this.wheelSize; i++)
                m_bucketArray[i] = new TimerList();
        }

        // 将move和dotask交给HierachicalTimeWheel完成
        // Tick仅仅改变current
        // 进位则返回true
        public bool Tick()
        {
            m_current++;
            bool carry =  m_current==wheelSize ;
            m_current %= wheelSize;
            return carry;
        }

        // 该timewheel上，现在走过的时间
        public TimeSpan GetCurrentTime()
        {
            return m_current * tickMs;
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
            m_bucketArray[(int)(timer.expire/tickMs)%wheelSize].Add(timer);
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
    }

    [AddComponentMenu("Timer/TimerManager")]
    public class TimerManager : MonoBehaviour 
    {
        // public field
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public static TimerManager? s_instance;
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        // timespan for one tick, 1ms
        public TimeSpan deltaTime;

        // private field
        // 由小到大
        // array[0] -> ms, array[1] -> s, ...
        private List<TimeWheel> m_timeWheelArray;
        private ConcurrentDictionary<uint, Timer> m_timerTable;
        private uint m_maxId = 0;
        private bool m_stop = true;
        private DateTime m_current;
        

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
            Init();
        }

        private void Init()
        {
            m_timeWheelArray = new List<TimeWheel>
            {
                // ms, s, min, hour, day
                new(new TimeSpan(0, 0, 0, 0, 1), 1000),
                new(new TimeSpan(0, 0, 0, 1), 60),
                new(new TimeSpan(0, 0, 1), 60),
                new(new TimeSpan(0, 1, 0), 24),
                new(new TimeSpan(1, 0, 0), 50)
            };
            deltaTime = new TimeSpan(0, 0, 0, 0, 1);
            m_timerTable = new ConcurrentDictionary<uint, Timer>();
            m_current = DateTime.Now;
        }

        private void Update()
        {
            DateTime now = DateTime.Now;
            while ( !m_stop && m_current<now )
            {
                Tick();
                m_current += deltaTime;
            }
        }

        // 完成所有在最小timewheel里的Timer
        // 将其他timewheels里当前的timer向下移
        // 重复任务重新放置
        private void Tick()
        {
            TimerList temp;
            for (int i=m_timeWheelArray.Count-1; i>0; i--) // 向下移动
            {
                temp = m_timeWheelArray[i].GetCurrentTimerList();
                while (temp.First!=null) this.RefreshTimer(temp.First);
            }
            // do tasks
            temp = m_timeWheelArray[0].GetCurrentTimerList();
            while (temp.First!=null) 
            {
                Timer t = temp.First;
                t.Callback();
                if ( t.times <= 1 )
                    this.RemoveTimer(t);
                else 
                {
                    this.ModifyTimer(t, t.interval+this.GetCurrentTime(),
                        t.interval, t.times-1);
                }
            }
            bool carry = true;
            foreach ( TimeWheel tw in m_timeWheelArray )
            {
                if ( !carry )
                    break;
                carry = tw.Tick();
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
                m_current = DateTime.Now;
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

        // total time span from starting running to current time
        public TimeSpan GetCurrentTime()
        {
            TimeSpan result = new();
            foreach ( TimeWheel timewheel in m_timeWheelArray )
                result += timewheel.GetCurrentTime();
            return result;
        }

        public TimeSpan GetMaxTime()
        {
            return m_timeWheelArray.Last().MaxTimeSpan;
        }

        // 把timer加入Dictionary
        // 根据时间把timer加入合适的timewheel
        public uint AddTimer(TimeSpan expire, TimeSpan interval, uint times, Action task)
        {
            Timer newTimer = new(GetAvaliableId(), expire, interval, times, task);
            this.AddTimer(newTimer);
            return newTimer.Id;
        }

        public void AddTimer(Timer timer)
        {
            timer.Id = GetAvaliableId();
            m_timerTable.TryAdd(timer.Id, timer);
            TimeSpan idx = timer.expire - GetCurrentTime();
            if ( timer.expire <= GetCurrentTime() )
                m_timeWheelArray.First().AddTimer(timer);
            else if ( idx >= GetMaxTime() )
                m_timeWheelArray.Last().AddTimer(timer);
            else
            {
                foreach ( TimeWheel tw in m_timeWheelArray )
                {
                    if ( idx < tw.MaxTimeSpan )
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
            m_timerTable.TryRemove(id, out timer);
            TimerList.Detach(timer);
            return timer;
        }

        public int RefreshTimer(Timer timer)
        {
            this.RemoveTimer(timer);
            this.AddTimer(timer);
            return 0;
        }

        public int ModifyTimer(Timer timer, TimeSpan expire, TimeSpan interval, uint times)
        {
            timer.expire = expire;
            timer.interval = interval;
            timer.times = times;
            this.RefreshTimer(timer);
            return 0;
        }

        public int ModifyTimer(uint id, TimeSpan expire, TimeSpan interval, uint times)
        {
            Timer modified = GetTimer(id);
            this.ModifyTimer(modified, expire, interval, times);
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