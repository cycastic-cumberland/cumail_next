using ExtendedComponents.Core;
using MongoDChatApp.ChatApp;
using MongoDChatApp.Database;

namespace MongoDChatApp;

public static class SelfInitializer
{
    private static MongoDConnectionSettings CreateConnectionSettings()
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

    public static MongoDChatAppQueryFactory CreateQueryFactory()
    {
        return new(CreateConnectionSettings());
    }

    private static void SafeBlock(Action action)
    {
        try
        {
            action();
        }
        catch (Exception)
        {
            // Ignored
        }
    }
    
    public static void SetupCollections()
    {
        var conn = CreateConnectionSettings();
        var provider = new MongoDProvider(conn);
        SafeBlock(() => provider.CreateCollection("user_profiles"));
        SafeBlock(() => provider.CreateCollection("chat_rooms_personnel"));
        SafeBlock(() => provider.CreateCollection("chat_rooms_info"));
        SafeBlock(() => provider.CreateCollection("chat_room_invitations"));
        SafeBlock(() => provider.CreateCollection("message_contents"));
        SafeBlock(() => provider.CreateCollection("message_reactions"));
    }
}