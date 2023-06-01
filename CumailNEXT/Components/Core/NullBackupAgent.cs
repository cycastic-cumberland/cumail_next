using CumailNEXT.Components.Auth;

namespace CumailNEXT.Components.Core;

public class NullBackupAgent : BackupAgent
{
    public NullBackupAgent() : base(new BackupAgentConfig
        {
            runFullRestoreOnStart = false,
            allowServer = false
        })
    {}

    public override void RunPartialBackup()
    {
    }

    public override void RunFullBackup()
    {
    }

    public override void RunPartialRestore()
    {
    }

    public override void RunFullRestore()
    {
    }
}