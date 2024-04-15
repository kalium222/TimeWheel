using System.Collections.Generic;

public abstract class ObjectPool<T> where T : new()
{
    private readonly LinkedList<T> m_AvailableList;
    public readonly int Level;

    // async?
    public ObjectPool(int initCount, int level)
    {
        Level = level;
        m_AvailableList = new();
        for (int i=0; i<initCount; i++)
            m_AvailableList.AddLast(new T());
    }
    protected abstract T Create(); 
    public T GetObject()
    {
        if ( m_AvailableList.Count < Level )
        {
            for (int i=0; i<Level; i++)
                m_AvailableList.AddLast(Create());
        }
        T result = m_AvailableList.Last.Value;
        m_AvailableList.RemoveLast();
        return result;
    }

    public void ReturnObject(T o)
    {
        m_AvailableList.AddLast(o);
    }
}
