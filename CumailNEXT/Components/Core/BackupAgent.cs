using CumailNEXT.Components.Paralex;

namespace CumailNEXT.Components.Auth;

public class BackupAgentConfig
{
    public string version = "1.0.0";
    public bool allowPartialBackup = false;
    public bool allowFullBackup = true;
    public bool allowServer = true;
    public bool runFullRestoreOnStart = false;
    public int partialBackupInterval = 10;
    public int fullBackupInterval = 30;
    public int serverInterval = 2000;
}

public abstract class BackupAgent
{
    private readonly SafeFlag exit;
    private readonly SafeFlag started;
    private readonly SafeFlag _paused;
    protected readonly BackupAgentConfig config;
    private readonly Thread backupThread;

    public abstract void RunPartialBackup();
    public abstract void RunFullBackup();
    public abstract void RunPartialRestore();
    public abstract void RunFullRestore();
    
    public bool BackupPaused
    {
        get => _paused.Flag;
        set => _paused.Flag = value;
    }
    public BackupAgent(BackupAgentConfig config)
    {
        exit = new(false);
        started = new(false);
        _paused = new SafeFlag(false);
        this.config = config;
        backupThread = new Thread(BackupServer);
        RunBootTask();
    }
    public void Terminate()
    {
        exit.Flag = true;
        backupThread.Join();
    }

    private void BackupServer()
    {
        started.Flag = true;
        DateTime currentTime = DateTime.UtcNow;
        long lastMinorInterval = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();
        long lastMajorInterval = lastMinorInterval;
        while (!exit.Flag)
        {
            if (_paused.Flag) continue;
            currentTime = DateTime.UtcNow;
            long currentInterval = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();
            if (config.allowFullBackup && currentInterval - lastMajorInterval >= config.fullBackupInterval)
            {
                RunFullBackup();
                lastMajorInterval = currentInterval;
            } else if (config.allowPartialBackup && currentInterval - lastMinorInterval >= config.partialBackupInterval)
            {
                RunPartialBackup();
                lastMinorInterval = currentInterval;
            }
            Thread.Sleep(config.serverInterval);
        }
    }

    private void RunBootTask()
    {
        if (config.runFullRestoreOnStart) RunFullRestore();
        if (config.allowServer) backupThread.Start();
    }
}