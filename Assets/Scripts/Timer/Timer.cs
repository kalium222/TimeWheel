using System;
using System.Collections;

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
        public Action callback;

        public Timer()
        {
            m_id = 0;
            expire = 0;
            interval = 0;
            times = 1;
            callback = () => {};
        }

        public Timer(uint id, TimeSpan expire, Action callback) 
        {
            m_id = id;
            this.expire = (uint)expire.TotalMilliseconds;
            interval = 0;
            times = 1;
            this.callback = callback;
        }

        public Timer(uint id, TimeSpan expire, TimeSpan interval, 
                uint times, Action callback) 
        {
            m_id = id;
            this.expire = (uint)expire.TotalMilliseconds;
            this.interval = (uint)interval.TotalMilliseconds;
            this.times = times;
            this.callback = callback;
        }

        public void Span2Uint(TimeSpan expire, TimeSpan interval)
        {
            this.expire = (uint)expire.TotalMilliseconds;
            this.interval = (uint)interval.TotalMilliseconds;
        }

        public void Destroy()
        {
            // GC will do the rest things
            Next?.Destroy();
            Next = null;
            Prev = null;
        }

        public uint Id {
            get { return m_id; }
            set { m_id = value; }
        }

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public Timer? Prev = null;
        public Timer? Next = null;
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        
        // 应该被HierachicalTimeWheel调用
        // 仅smallest timewheel需DoTask
        // 还需要重新调度
        public void Callback()
        {
            callback();
        }
    }

    public class TimerList : IEnumerable
    {
        private readonly Timer m_head;
        
        // public method
        public TimerList()
        {
            m_head = new Timer();
        }

        private class TimerIterator : IEnumerator
        {
            private readonly TimerList m_container;
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
            private Timer? m_postion;
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
            public TimerIterator(TimerList container)
            {
                m_container = container;
                m_postion = container.Head;
            }
            void IEnumerator.Reset()
            {
                m_postion = m_container.Head;
            }
            bool IEnumerator.MoveNext()
            {
                m_postion = m_postion?.Next;
                return m_postion != null;
            }
            object IEnumerator.Current
            {
                get
                {
                    if ( m_postion==null )
                        throw new IndexOutOfRangeException();
                    return m_postion;
                }
            }
        }

        public IEnumerator GetEnumerator()
        {
            return new TimerIterator(this);
        }

        // 不检查各种条件(是否重复等)，交给timewheel
        // 及heirachical timewheels
        public void Add(Timer timer)
        {
            timer.Prev = m_head;
            timer.Next = m_head.Next;
            if ( timer.Next != null ) 
                timer.Next.Prev = timer;
            m_head.Next = timer;
        }

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public static void Detach(Timer? timer)
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        {
            if ( timer==null )
                return;
            if ( timer.Next != null )
                timer.Next.Prev = timer.Prev;
            if ( timer.Prev != null )
                timer.Prev.Next = timer.Next;
            timer.Next = timer.Prev = null;
        }

        public void Clear()
        {
            m_head.Next?.Destroy();
            m_head.Next = null;
        }

        public Timer Head => m_head;

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public Timer? First => m_head.Next;
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

        public int Count
        {
            get
            {
                int result = 0;
                foreach ( Timer t in this )
                    result++;
                return result;
            }
        }
    }
}
