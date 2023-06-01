namespace CumailNEXT.Components.Auth;

public class NoAuthBackup : AuthBackupAgent
{
    public NoAuthBackup(BackupAgentConfig config, AuthQuery query) : base(new BackupAgentConfig
    {
        runFullRestoreOnStart = false,
        allowServer = false
    }, query)
    {}
}

public abstract class AuthBackupAgent : BackupAgent
{
    protected readonly AuthQuery query;
    public AuthBackupAgent(BackupAgentConfig config, AuthQuery query) : base(config)
    {
        this.query = query;
    }

    public override void RunPartialBackup()
    {
        throw new NotImplementedException();
    }

    public override void RunFullBackup()
    {
        throw new NotImplementedException();
    }

    public override void RunPartialRestore()
    {
        throw new NotImplementedException();
    }

    public override void RunFullRestore()
    {
        throw new NotImplementedException();
    }
}