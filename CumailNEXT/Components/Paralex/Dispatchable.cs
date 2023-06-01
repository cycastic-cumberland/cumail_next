namespace CumailNEXT.Components.Paralex;

public class Dispatchable
{
    private readonly Action process;
    private readonly SafeFlag isFinished;
    private readonly SafeFlag dequeued;
    
    public Dispatchable(Action process)
    {
        this.process = process;
        isFinished = new SafeFlag(false);
        dequeued = new SafeFlag(false);
    }
    public void SetFinished() { isFinished.Flag = true; }
    public Action GetProcess() { return process; }
    public void WaitToFinish() { isFinished.WaitToFinish(); }

    public bool IsDequeued() { return dequeued.Flag; }
    public void SetDequeued() { dequeued.Flag = true; }
}