namespace CoreComponents.Paralex;

public class CommandQueue : IDisposable
{
    private class QueuedTask
    {
        public readonly TaskCompletionSource<bool> Source;
        public readonly Action TaskAction;
        
        public QueuedTask(Action action)
        {
            TaskAction = action;
            Source = new();
        }

        public async Task Wait()
        {
            await Source.Task;
        }
    }
    private readonly Thread _server;
    private readonly Queue<QueuedTask> _taskQueue = new();
    private readonly object _conditionalLock = new();
    private bool _terminationFlag;

    public CommandQueue()
    {
        _server = new Thread(Start);
        _server.Start();
    }

    private void Start()
    {
        while (!_terminationFlag)
        {
            QueuedTask task;
            lock (_conditionalLock)
            {
                while (_taskQueue.Count == 0 && !_terminationFlag)
                    Monitor.Wait(_conditionalLock);
                if (_terminationFlag)
                {
                    foreach (var t in _taskQueue)
                    {
                        t.Source.SetCanceled();
                    }
                    _taskQueue.Clear();
                    return;
                }
                
                task = _taskQueue.Dequeue();
            }

            try
            {
                task.TaskAction.Invoke();
                task.Source.SetResult(true);
            }
            catch (Exception ex)
            {
                task.Source.SetException(ex);
            }
        }
    }

    public Task DispatchTask(Action action)
    {
        lock (_conditionalLock)
        {
            QueuedTask queued = new(action);
            _taskQueue.Enqueue(queued);
            Monitor.Pulse(_conditionalLock);
            return queued.Source.Task;
        }
    }

    public void SyncTask(Action action)
    {
        DispatchTask(action).Wait();
    }

    public void Terminate()
    {
        lock (_conditionalLock) {
            _terminationFlag = true;
            Monitor.Pulse(_conditionalLock);
        }   
    }
    public void Dispose()
    {
        Terminate();
        _server.Join();
    }
}