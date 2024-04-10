using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework.Internal;
using Timer;

public class TestTimer
{
    // Test helper classes

    // Timer
    [Test]
    public void TestTimerConstructor()
    {
        // Test Timer()
        Timer.Timer t1 = new();
        Assert.IsTrue(t1.Id==0);
        Assert.IsTrue(t1.expire==new TimeSpan(0));
        Assert.IsTrue(t1.interval==new TimeSpan(0));
        Assert.IsTrue(t1.times==1);
        // Test Timer(uint id, TimeSpan expire, Action callback)
        Timer.Timer t2 = new(8, new TimeSpan(1, 1, 1), ()=>{});
        Assert.IsTrue(t2.Id==8);
        Assert.IsTrue(t2.expire==new TimeSpan(1, 1, 1));
        Assert.IsTrue(t2.interval==new TimeSpan());
        Assert.IsTrue(t2.times==1);
        // Test Timer(uint id, TimeSpan expire, TimeSpan interval, 
        //          uint times, Action callback)
        Timer.Timer t3 = new(111, new TimeSpan(3, 3, 3),
                    new TimeSpan(7, 7, 7),  114514, ()=>{});
        Assert.IsTrue(t3.Id==111);
        Assert.IsTrue(t3.expire==new TimeSpan(3, 3, 3));
        Assert.IsTrue(t3.interval==new TimeSpan(7, 7, 7));
        Assert.IsTrue(t3.times==114514);
    }

    [Test]
    public void TestTimerDestroy()
    {
        Timer.Timer t2 = new();
        Timer.Timer t3 = new();
        t2.Next = t3;
        t3.Prev = t2;
        Assert.IsNotNull(t2.Next);
        Assert.IsNotNull(t3.Prev);
        t2.Destroy();
        Assert.IsNull(t2.Next);
        Assert.IsNull(t3.Prev);
    }

    [Test]
    public void TestTimerCallback()
    {
        int a = 1;
        Timer.Timer t = new(1, new(), () => {
            a++;
        });
        Assert.IsTrue(a==1);
        t.Callback();
        Assert.IsTrue(a==2);
    }

    // TimerList
    [Test]
    public void TestTimerListIterator()
    {
        TimerList l = new();
        for (int i=0; i<100; i++)
            l.Add(new Timer.Timer((uint)i, new TimeSpan(1, 1, 1), ()=>{}));
        int index = 0;
        foreach ( Timer.Timer t in l ) 
        {
            Assert.IsTrue(t.Id==(99-index));
            index++;
        }
        Assert.IsTrue(l.Count==100);
    }

    [Test]
    public void TestTimerListDetach()
    {
        TimerList l = new();
        List<Timer.Timer> list = new();
        for (int i=0; i<100; i++) 
        {
            Timer.Timer t = new((uint)i, new TimeSpan(1, 1, 1), ()=>{});
            l.Add(t);
            list.Add(t);
        }
        Assert.IsTrue(l.Count==100);
        for (int i=0; i<100; i++)
        {
            TimerList.Detach(list[99-i]);
            Assert.IsTrue(l.Count==99-i);
        }
    }

    // TimeWheel
    [Test]
    public void TestTimeWheelTick()
    {
        TimeSpan tickMs = new(0, 0, 1);
        TimeWheel wheel = new(tickMs, 60);
        for (int i=0; i<59; i++)
        {
            Assert.IsTrue(wheel.GetCurrentTime()==i*tickMs);
            Assert.IsFalse(wheel.Tick());
        }
        Assert.IsTrue(wheel.Tick());
    }

    [Test]
    public void TestTimeWheelGetDistri()
    {
        System.Random rd = new();
        TimeSpan tickMs = new(0, 0, 1);
        int wheelSize = 60;
        TimeWheel wheel = new(tickMs, wheelSize);
        int[] distri = new int[wheelSize];
        for (int i=0; i<wheelSize; i++)
        {
            distri[i] = rd.Next(1, 100);
            for (int j=0; j<distri[i]; j++)
            {
                Timer.Timer t = new(0, i*tickMs, ()=>{});
                wheel.AddTimer(t);
            }
        }
        for (int i=0; i<wheelSize; i++)
        {
            Assert.IsTrue(distri[i]==wheel.GetDistri()[i]);
        }
    }

    [Test]
    public void TestTimeWheelClearAll()
    {
        System.Random rd = new();
        TimeSpan tickMs = new(0, 0, 1);
        int wheelSize = 60;
        TimeWheel wheel = new(tickMs, wheelSize);
        int[] distri = new int[wheelSize];
        for (int i=0; i<wheelSize; i++)
        {
            distri[i] = rd.Next(1, 100);
            for (int j=0; j<distri[i]; j++)
            {
                Timer.Timer t = new(0, i*tickMs, ()=>{});
                wheel.AddTimer(t);
            }
        }
        for (int i=0; i<wheelSize; i++)
        {
            Assert.IsTrue(distri[i]==wheel.GetDistri()[i]);
        }
        wheel.ClearAll();
        for (int i=0; i<wheelSize; i++)
        {
            Assert.IsTrue(wheel.GetDistri()[i]==0);
        }
    }

}

public class TestTimerManager
{
    [UnityTest]
    public IEnumerator TestTimerManagerWithEnumeratorPasses()
    {
        GameObject timerManagerObject = new();
        Assert.IsNull(TimerManager.s_instance);

        timerManagerObject.AddComponent<TimerManager>();
        yield return null;
        TimerManager instance = TimerManager.s_instance;
        Assert.IsNotNull(instance);
        yield return null;
    }
}
