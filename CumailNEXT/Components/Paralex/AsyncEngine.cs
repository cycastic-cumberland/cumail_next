using AuthModule.Paralex;

namespace CumailNEXT.Components.Paralex;

public class AsyncEngine : IDisposable
{
    public const int ModeHot = 0;
    public const int ModeCold = 1;
    public const int ModeOnSpot = 2;

    public int QueueInterval { get; set; } = 1;
        
    private readonly CommandQueue queue;
    private readonly SafeFlag exit;
    private readonly Thread serverThread;
    private readonly int heatMode;
    private int serverId = 0;

    public AsyncEngine(int heatMode, bool daemonMode)
    {
        queue = new CommandQueue();
        exit = new SafeFlag(daemonMode);
        serverThread = new Thread(Server);
        if (heatMode < ModeHot || heatMode > ModeOnSpot) this.heatMode = ModeHot;
        else this.heatMode = heatMode;
        if (heatMode == ModeHot) {
            serverThread.IsBackground = daemonMode;
            serverThread.Start();
        }
    }
    public AsyncEngine(int heatMode) : this(heatMode, false) {}
    public AsyncEngine(bool daemonMode) : this(ModeHot, daemonMode) {}
    public AsyncEngine() : this(false) {}
    public void Server()
    {
        serverId = Environment.CurrentManagedThreadId;
        while (!exit.Flag)
        {
            queue.ExecuteOne();
            if (QueueInterval > 0) Thread.Sleep(QueueInterval);
        }
        queue.ExecuteAll();
    }
    public int ServerId => serverId;
    public int QueueSize() { return queue.QueueSize(); }
    public void Dispose()
    {
        if (heatMode != ModeHot || exit.Flag) { return; }
        if (Environment.CurrentManagedThreadId == serverId)
        {
            new Thread(Dispose).Start();
            return;
        }
        exit.Flag = true;
        while (serverThread.IsAlive);
    }
    public void Dispatch(Action action)
    {
        switch (heatMode)
        {
            case ModeHot: queue.Dispatch(action); break;
            case ModeCold: new Thread(new ThreadStart(action)).Start(); break;
            case ModeOnSpot: action(); break;
        }
    }
    public void Sync(Action action)
    {
        switch (heatMode)
        {
            case ModeHot:
            {
                if (Environment.CurrentManagedThreadId == serverId)
                    throw new ConcurrencyException("Trying to sync a task while being executed asynchronously.");
                queue.Sync(action);
                break;
            }
            case ModeCold:
            {
                SafeFlag finished = new SafeFlag(false);
                new Thread(() =>
                {
                    action();
                    finished.Flag = true;
                }).Start();
                finished.WaitToFinish();
                break;
            }
            case ModeOnSpot: action(); break;
        }
    }

    public void Sync()
    {
        Sync(() => { });
    }
}