using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Timer;

public class TestTimerManager
{
    // A Test behaves as an ordinary method
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

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator TestTimerManagerWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        Assert.IsTrue(true);
        yield return null;
    }
}
