using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;

namespace Timer
{

    public static class TimerConstants
    {
        // timespan for one tick, 1ms
        public static readonly TimeSpan k_DeltaTime = new(0, 0, 0, 0, 1);
        public static readonly TimeSpan k_MaxTime = new(0, 0, 0, 
                    (int)(uint.MaxValue/1000), (int)(uint.MaxValue%1000));

        // 32bit = 8bit(rootwheel) + 4 * 6bit(others)
        public const int k_TWRootBits = 8;
        public const uint k_TWRootSize = 1 << k_TWRootBits; 
        public const uint k_TWRootMask = k_TWRootSize - 1;
        public const int k_TWBits = 6;
        public const uint k_TWSize = 1 << k_TWBits; 
        public const uint k_TWMask = k_TWSize - 1;
    }

    public class TimerPool : ObjectPool<Timer>
    {
        public TimerPool(int initCount, int level) : base(initCount, level) {}

        protected override Timer Create()
        {
            return new();
        }
    }

    public class TimeWheel
    {
        // private field
        public readonly TimerList[] BucketArray;

        // public field
        public readonly int wheelSize;
        
        // public method
        public TimeWheel(int wheelSize)
        {
            this.wheelSize = wheelSize;
            BucketArray = new TimerList[wheelSize];
            for (int i=0; i< this.wheelSize; i++)
                BucketArray[i] = new TimerList();
        }

        public List<int> GetDistri()
        {
            List<int> res = new();
            foreach ( TimerList l in BucketArray )
            {
                res.Add(l.Count);
            }
            return res;
        }

        public void ClearAll()
        {
            foreach ( TimerList l in BucketArray )
                l.Clear();
        }

        public int Count 
        {
            get
            {
                int result = 0;
                foreach ( TimerList l in BucketArray )
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

        // private field
        private TimerPool m_timerPool;
        private List<TimeWheel> m_timeWheelArray;
        
        private ConcurrentDictionary<uint, Timer> m_timerTable;
        private uint m_maxId = 0;
        private bool m_stop = true;
        private DateTime m_current;
        private uint m_currentTick;
        

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
            m_timerPool = new(10_0000, 1_0000);
            m_timeWheelArray = new List<TimeWheel>
            {
                // 32bit = 8bit(rootwheel) + 4 * 6bit(others)
                new((int)TimerConstants.k_TWRootSize),
                new((int)TimerConstants.k_TWSize),
                new((int)TimerConstants.k_TWSize),
                new((int)TimerConstants.k_TWSize),
                new((int)TimerConstants.k_TWSize)
            };
            m_timerTable = new ConcurrentDictionary<uint, Timer>();
            m_current = DateTime.Now;
            m_currentTick = 0;
        }

        private void Update()
        {
            DateTime now = DateTime.Now;
            while ( !m_stop && m_current<now )
            {
                Tick();
                m_current += TimerConstants.k_DeltaTime;
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
                temp = m_timeWheelArray[i].BucketArray[Index(i)];
                while (temp.First!=null) RefreshTimer(temp.First);
            }
            // do tasks
            temp = m_timeWheelArray[0].BucketArray[m_currentTick&TimerConstants.k_TWRootMask];
            while (temp.First!=null) 
            {
                Timer t = temp.First;
                t.Callback();
                if ( t.times <= 1 )
                    RemoveTimer(t.Id);
                else 
                {
                    ModifyTimer(t, t.interval+m_currentTick,
                        t.interval, t.times-1);
                }
            }
            m_currentTick++;
        }

        private uint GetAvaliableId()
        {
            if ( m_maxId == uint.MaxValue )
                m_maxId = 0;
            while ( m_timerTable.ContainsKey(m_maxId) )
                m_maxId++;
            return m_maxId;
        }

        private int Index(int n)
        {
            return (int)((m_currentTick >> (TimerConstants.k_TWRootBits + (n-1) * TimerConstants.k_TWBits)) & TimerConstants.k_TWMask);
        }

        public List<TimeWheel> TimeWheelArray => m_timeWheelArray;

        // return true if started succesfully
        // return false if instance has already
        // been launched
        public bool StartRunning()
        {
            if ( m_stop )
            {
                m_stop = false;
                m_current = DateTime.Now;
                m_currentTick = 0;
                return true;
            }
            else 
            {
                return false;
            }
        }

        public bool IsRunning => !m_stop;

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public Timer? GetTimer(uint id)
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        {
            if ( !m_timerTable.TryGetValue(id, out Timer timer) )
                return null;
            return timer;
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
            return new TimeSpan(0, 0, 0, (int)(m_currentTick/1000), 
                                    (int)(m_currentTick%1000));
        }

        // Get timer from pool
        // 把timer加入Dictionary
        // 根据时间把timer加入合适的timewheel
        public uint AddTimer(TimeSpan expire, TimeSpan interval,
                                uint times, int a, int b)
        {
            Timer timer = m_timerPool.GetObject();
            timer.Span2Uint(expire, interval);
            timer.times = times;
            timer.a = a;
            timer.b = b;
            timer.Id = GetAvaliableId();
            m_timerTable.TryAdd(timer.Id, timer);
            AddTimerToWheel(timer);
            return timer.Id;
        }
        public uint AddTimer(TimeSpan expire, TimeSpan interval,
                                uint times, int a, int b,
                                Action<int, int> callback)
        {
            Timer timer = m_timerPool.GetObject();
            timer.Span2Uint(expire, interval);
            timer.times = times;
            timer.a = a;
            timer.b = b;
            timer.callback = callback;
            timer.Id = GetAvaliableId();
            m_timerTable.TryAdd(timer.Id, timer);
            AddTimerToWheel(timer);
            return timer.Id;
        }

        private void AddTimerToWheel(Timer timer)
        {
            ulong idx = timer.expire - m_currentTick;
            TimerList target;
            if ( (long)idx <= 0 )
                target = m_timeWheelArray[0].BucketArray[((m_currentTick & TimerConstants.k_TWRootMask) + 1)%TimerConstants.k_TWRootSize];
            else if (idx < TimerConstants.k_TWRootSize)
                target = m_timeWheelArray[0].BucketArray[timer.expire & TimerConstants.k_TWRootMask];
            else if (idx < 1 << (TimerConstants.k_TWRootBits+TimerConstants.k_TWBits))
                target = m_timeWheelArray[1].BucketArray[timer.expire>>(TimerConstants.k_TWRootBits) & TimerConstants.k_TWMask];
            else if (idx < 1 << (TimerConstants.k_TWRootBits+2*TimerConstants.k_TWBits))
                target = m_timeWheelArray[2].BucketArray[timer.expire>>(TimerConstants.k_TWRootBits+TimerConstants.k_TWBits) & TimerConstants.k_TWMask];
            else if (idx < 1 << (TimerConstants.k_TWRootBits+3*TimerConstants.k_TWBits))
                target = m_timeWheelArray[3].BucketArray[timer.expire>>(TimerConstants.k_TWRootBits+TimerConstants.k_TWBits*2) & TimerConstants.k_TWMask];
            else if (idx < 1 << (TimerConstants.k_TWRootBits+4*TimerConstants.k_TWBits))
                target = m_timeWheelArray[4].BucketArray[timer.expire>>(TimerConstants.k_TWRootBits+TimerConstants.k_TWBits*3) & TimerConstants.k_TWMask];
            else
                target = m_timeWheelArray[4].BucketArray[TimerConstants.k_TWSize-1];
            target.Add(timer);
        }

        // only detach a timer in the wheel
        // should be call by RemoveTimer()
        // or Modify() or Refresh()
        private void DetachTimerFromWheel(Timer timer)
        {
            foreach (var wheel in m_timeWheelArray)
            {
                foreach (var list in wheel.BucketArray)
                {
                    list.Detach(timer);
                }
            }
        }

        // Remove a timer from dictionary;
        // Detach it from wheel
        // Return the timer to the pool
        // return false if it doesn't exist in the dictionary
        public bool RemoveTimer(uint id)
        {
            bool state = m_timerTable.TryRemove(id, out Timer timer);
            if (state)
            {
                DetachTimerFromWheel(timer);
                m_timerPool.ReturnObject(timer);
            }
            return state;
        }

        // Refresh the position of a timer
        // if it should be move to a lower wheel
        private void RefreshTimer(Timer timer)
        {
            DetachTimerFromWheel(timer);
            AddTimerToWheel(timer);
        }

        private void ModifyTimer(Timer timer, uint expire, uint interval,
                                     uint times)
        {
            timer.expire = expire;
            timer.interval = interval;
            timer.times = times;
            RefreshTimer(timer);
        }

        private void ModifyTimer(Timer timer, uint expire, uint interval,
                                     uint times, int a, int b)
        {
            timer.expire = expire;
            timer.interval = interval;
            timer.times = times;
            timer.a = a;
            timer.b = b;
            RefreshTimer(timer);
        }

        // return false if it doesn't contain the key
        public bool ModifyTimer(uint id, TimeSpan expire, TimeSpan interval,
                                    uint times, int a, int b)
        {
            Timer modified = GetTimer(id);
            if ( modified==null ) return false;
            ModifyTimer(modified, 0, 0, times, a, b);
            modified.Span2Uint(expire, interval);
            return true;
        }

        public void Reset()
        {
            m_stop = true;
            m_currentTick = 0;
            foreach ( TimeWheel tw in m_timeWheelArray )
            {
                foreach ( TimerList l in tw.BucketArray )
                {
                    while ( l.First != null )
                    {
                        Timer t = l.First;
                        RemoveTimer(t.Id);
                    }
                }
            }
         }

    }

}