using System;
using System.Collections;
using System.Collections.Generic;

namespace Timer
{
    public class Timer
    {
        // private
        private uint m_id;

        // public
        // time span from being added into the TimerManager to when
        // it should be executed
        public uint expire;
        public uint interval;
        public uint times;
        public Action<int, int> callback = (int a, int b) => {};
        public int a = 0, b = 0;

        public Timer()
        {
            m_id = 0;
            expire = 0;
            interval = 0;
            times = 1;
        }

        public Timer(uint id, TimeSpan expire) 
        {
            m_id = id;
            this.expire = (uint)expire.TotalMilliseconds;
            interval = 0;
            times = 1;
        }

        public Timer(uint id, TimeSpan expire, TimeSpan interval, 
                uint times) 
        {
            m_id = id;
            this.expire = (uint)expire.TotalMilliseconds;
            this.interval = (uint)interval.TotalMilliseconds;
            this.times = times;
        }

        public void Span2Uint(TimeSpan expire, TimeSpan interval)
        {
            this.expire = (uint)expire.TotalMilliseconds;
            this.interval = (uint)interval.TotalMilliseconds;
        }

        public uint Id {
            get { return m_id; }
            set { m_id = value; }
        }

        // 应该被HierachicalTimeWheel调用
        // 仅smallest timewheel需DoTask
        // 还需要重新调度
        public void Callback()
        {
            callback(a, b);
        }
    }

    public class TimerList 
    {
        public readonly LinkedList<Timer> List = new();

        // public method

        // 不检查各种条件(是否重复等)，交给timewheel
        // 及heirachical timewheels
        public void Add(Timer timer)
        {
            List.AddLast(timer);
        }

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public void Detach(Timer? timer)
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        {
            if ( List.Contains(timer))
                List.Remove(timer);
        }

        public void Clear()
        {
            List.Clear();
        }

        public Timer? First => List.First?.Value ?? null;

        public int Count => List.Count;
    }
}
