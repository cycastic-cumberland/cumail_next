namespace AuthModule.Paralex;

internal class QueueNode<T>
{
    public readonly T value;
    public QueueNode<T>? next;
    public QueueNode(T value)
    {
        this.value = value;
    }
}
public class FIFOQueue<T>
{
    private QueueNode<T>? first;
    private QueueNode<T>? last;
    private int _cachedSize = 0;
    public int Count => Size();
    public int Size()
    {
        return _cachedSize;
    }

    public bool IsEmpty()
    {
        return Size() == 0;
    }

    public void Clear()
    {
        while (!IsEmpty()) Dequeue();
    }
    public void Enqueue(T value)
    {
        QueueNode<T> newNode = new QueueNode<T>(value);
        if (first == null || last == null)
        {
            first = newNode;
            last = newNode;
        } else
        {
            last.next = newNode;
            last = newNode;
        }
        _cachedSize++;
    }
    public T Dequeue()
    {
        QueueNode<T>? iter = first;
        if (iter == null) throw new IndexOutOfRangeException();
        T returnValue = iter.value;
        first = iter.next;
        if (first == null) last = null;
        _cachedSize--;
        return returnValue;
    }
}