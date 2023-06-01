using CumailNEXT.Components.Paralex;

namespace AuthModule.Paralex;

public class CommandQueue
{
    private readonly FIFOQueue<Dispatchable> queue;
    private readonly SafeFlag tolarable;

    public bool IsFaultTolerant
    {
        get => tolarable.Flag;
        set => tolarable.Flag = value;
    }
    public CommandQueue()
    {
        queue = new FIFOQueue<Dispatchable>();
        tolarable = new SafeFlag(true);
    }

    public int QueueSize()
    {
        lock (this)
        {
            return queue.Size();
        }
    }
    public bool ExecuteOne()
    {
        lock (this)
        {
            if (queue.Count == 0) return false;
            Dispatchable first = queue.Dequeue();
            first.SetDequeued();
            if (IsFaultTolerant)
            {
                try
                {
                    first.GetProcess()();
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine("Uncaught exception during queued execution: " +
                                            $"{e.Message}\nStack Trace:\n{e.StackTrace}");
                }
            } else first.GetProcess()();
            first.SetFinished();
            return true;
        }
    }
    public void ExecuteAll()
    {
        while (ExecuteOne())
        {
            
        }
    }
    public Dispatchable Dispatch(Action action)
    {
        lock (this)
        {
            Dispatchable re = new Dispatchable(action);
            queue.Enqueue(re);
            return re;
        }
    }
    public void Sync(Action action)
    {
        Dispatch(action).WaitToFinish();
    }
}