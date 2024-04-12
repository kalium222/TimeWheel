using System.Collections.Generic;

public abstract class ObjectPool<T> where T : new()
{
    private readonly LinkedList<T> m_AvailableList;
    private const int k_level = 1000;
    
    // async?
    public ObjectPool(int initCount)
    {
        m_AvailableList = new();
        for (int i=0; i<initCount; i++)
        {
            m_AvailableList.AddLast(new T());
        }
    }

    protected abstract T Create(); 

    public T GetObject()
    {
        if ( m_AvailableList.Count < k_level )
        {
            for (int i=0; i<k_level; i++)
                Create();
        }
        T result = m_AvailableList.Last.Value;
        m_AvailableList.RemoveLast();
        return result;
    }
}
