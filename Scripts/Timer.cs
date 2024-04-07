using System;
using System.Collections;

namespace Timer
{
    public class Timer
    {
        // private
        private uint m_id;
        // 绝对时间
        public ulong expire;
        public ulong interval;
        public uint times;

        // public
        public event Action Task;

        public Timer()
        {
            m_id = 0;
            expire = 0;
            interval = 0;
            times = 1;
            Task += () => {};
        }

        public Timer(uint id)
        {
            m_id = id;
            expire = 0;
            interval = 0;
            times = 1;
            Task += () => {};
        }

        public Timer(uint id, ulong postpone, Action task) 
        {
            m_id = id;
            this.expire = postpone;
            interval = 0;
            times = 1;
            Task += task;
        }

        public Timer(uint id, ulong postpone, ulong interval, uint times, Action task) 
        {
            m_id = id;
            this.expire = postpone;
            this.interval = interval;
            this.times = times;
            Task += task;
        }

        public void Destroy()
        {
            if ( Next!=null ) Next.Destroy();
            Next = null;
            Prev = null;
        }

        public uint Id {
            get { return m_id; }
            set { m_id = value; }
        }
        
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public Timer? Prev { get; set; }
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public Timer? Next { get; set; }
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

        // 应该被HierachicalTimeWheel调用
        // 仅smallest timewheel需DoTask
        // 还需要重新调度
        public void DoTask()
        {
            Task.Invoke();
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
                m_postion = m_postion != null ? m_postion.Next : null;
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

        public Timer Head
        {
            get { return m_head; }
        }

        public int Count
        {
            get
            {
                int result = 0;
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
                Timer? p = m_head.Next;
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
                while ( p!=null )
                {
                    result++;
                    p = p.Next;
                }
                return result;
            }
        }
    }

}
