using ExtendedComponents.Core;
using PostgresChatApp.ChatApp;
using PostgresChatApp.Database;

namespace PostgresChatApp;

public static class SelfInitializer
{
    private static PostgresConnectionSettings CreateConnectionSettings()
    {
        return new()
        {
            Address = ProjectSettings.Instance.Get(SettingsCatalog.ChatHubDbAddress, ""),
            DatabaseName = ProjectSettings.Instance.Get(SettingsCatalog.ChatHubDbDatabaseName, ""),
            Port = ProjectSettings.Instance.Get(SettingsCatalog.ChatHubDbPort, 0),
            Username = ProjectSettings.Instance.Get(SettingsCatalog.ChatHubDbUsername, ""),
            Password = ProjectSettings.Instance.Get(SettingsCatalog.ChatHubDbPassword, "")
        };
    }
    public static PostgresChatAppQueryFactory CreateQueryFactory()
    {
        return new(CreateConnectionSettings());
    }
}