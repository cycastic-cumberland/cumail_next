using Auth;
using ChatApp;
using ChatApp.Tables;
using CoreComponents.Core;
using ExtendedComponents.Core;
using PostgresChatApp.ChatApp;
using PostgresChatApp.Database;

namespace PostgresChatApp;

public class SelfInitializer
{
    private readonly ChatAppQueryFactory _queryFactory;
    public SelfInitializer(ChatAppQueryFactory factory) => _queryFactory = factory;
    private static PostgresConnectionSettings CreateConnectionSettings()
    {
        return new()
        {
            Address = ProjectSettings.Instance.Get(SettingsCatalog.ChatHubConnectionAddress, ""),
            DatabaseName = ProjectSettings.Instance.Get(SettingsCatalog.ChatHubConnectionDatabaseName, ""),
            Port = ProjectSettings.Instance.Get(SettingsCatalog.ChatHubConnectionPort, 0),
            Username = ProjectSettings.Instance.Get(SettingsCatalog.ChatHubConnectionUsername, ""),
            Password = ProjectSettings.Instance.Get(SettingsCatalog.ChatHubConnectionPassword, "")
        };
    }
    public static PostgresChatAppQueryFactory CreateQueryFactory()
    {
        return new PostgresChatAppQueryFactory(CreateConnectionSettings());
    }

    public AuthProvider.SignupIntervention Inject(AuthUser user)
    {
        try
        {
            using ChatAppQuery appQuery = _queryFactory.CreateInstance();
            var success = false;
            appQuery.OpenTransaction((query, _) =>
            {
                var profile = query.GetUserById(user.UserUuid);
                if (profile != null)
                {
                    query.RemoveUser(user.UserUuid);
                }

                var usernameRaw = user.UserLoginKey.Split('@')[0];
                var username = usernameRaw.Length <= 32 ? usernameRaw : Crypto.HashSha256String(user.UserLoginKey);
                query.AddUser(new UserProfile
                {
                    UserId = user.UserUuid,
                    UserName = username
                });
                success = true;
            });
            return success ? AuthProvider.SignupIntervention.Allow : AuthProvider.SignupIntervention.Reject;
        }
        catch (Exception)
        {
            return AuthProvider.SignupIntervention.Reject;
        }
    }
}