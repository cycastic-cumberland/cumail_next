using System.Reflection;
using Auth;
using ChatApp;
using ChatApp.Tables;
using CoreComponents.Core;
using CumailNEXT.Components.ChatApp;
using CumailNEXT.Implementation.ChatApp;
using ExtendedComponents.Core;
using Newtonsoft.Json;
using PostgresChatApp.ChatApp;
using PostgresChatApp.Database;
using RedisAuth.Auth;
using RedisAuth.Database;

namespace CumailNEXT.Implementation.Core;

public class Engine
{
    private static readonly object InstanceLock = new double();
    private static RedisProvider? _redis = null;
    private static AuthProvider? _auth = null;
    private static PostgresChatAppQueryFactory? _appQueryFactory = null;
    private static QueuedChatHubStorage? _hubStorageFactory = null;
    private static MonolithicChatAppFactory? _chatAppFactory = null;
    private static byte[]? _tokenIV = null;
    public const string ConfigFileName = "config.json";
    public static RedisProvider RedisDb
    {
        get
        {
            lock (InstanceLock)
            {
                _redis ??= new RedisProvider(new RedisConnectionConfig
                {
                    Address = ProjectSettings.Instance.Get(SettingsCatalog.AuthDbEndpoint, ""),
                    Username = ProjectSettings.Instance.Get(SettingsCatalog.AuthDbUsername, ""),
                    Password = ProjectSettings.Instance.Get(SettingsCatalog.AuthDbPassword, "")
                });
            }

            return _redis;
        }
    }

    public static PostgresConnectionSettings PostgresConnectionSettings { get; } = new()
    {
        Address = ProjectSettings.Instance.Get(SettingsCatalog.ChatHubConnectionAddress, ""),
        DatabaseName = ProjectSettings.Instance.Get(SettingsCatalog.ChatHubConnectionDatabaseName, ""),
        Port = ProjectSettings.Instance.Get(SettingsCatalog.ChatHubConnectionPort, 0),
        Username = ProjectSettings.Instance.Get(SettingsCatalog.ChatHubConnectionUsername, ""),
        Password = ProjectSettings.Instance.Get(SettingsCatalog.ChatHubConnectionPassword, "")
    };

    public static PostgresChatAppQueryFactory AppQueryFactory
    {
        get
        {
            lock (InstanceLock)
            {
                _appQueryFactory ??= new PostgresChatAppQueryFactory(PostgresConnectionSettings);
            }

            return _appQueryFactory;
        }
    }

    public static PostgresChatAppQuery NewAppQuery => AppQueryFactory.NewQueryInstance();
    public static AuthProvider Auth
    {
        get
        {
            lock (InstanceLock)
            {
                if (_auth != null) return _auth;
                _auth = new ModularAuthProvider(new MonolithicRedisAuthQuery(RedisDb), ProjectSettings.Instance.Get(SettingsCatalog.AuthTokenSecret, ""));
                _auth.OnSignupSuccess(user =>
                {
                    try
                    {
                        using ChatAppQuery appQuery = NewAppQuery;
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
                });

            }

            return _auth;
        }
    }
    public static MonolithicChatAppFactory ChatAppFactory
    {
        get
        {
            lock (InstanceLock)
            {
                _chatAppFactory ??= new MonolithicChatAppFactory(Auth, AppQueryFactory);
            }

            return _chatAppFactory;
        }
    }

    public static QueuedChatHubStorage HubStorageFactory
    {
        get
        {
            lock (InstanceLock)
            {
                // TODO: Check this out lol
                _hubStorageFactory ??= new NotQueuedChatHubStorage();
            }

            return _hubStorageFactory;
        }
    }

    public static byte[] TokenInitVector
    {
        get
        {
            lock (InstanceLock)
            {
                _tokenIV ??= Crypto.GenerateSecureBytes(16);
            }

            return _tokenIV;
        }
    }

    public static void Init()
    {
        // Enroll configs
        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (path == null) throw new NullReferenceException();
        var configPath = Path.Join(path, ConfigFileName);
        string content = File.ReadAllText(configPath);
        var asDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
        if (asDict == null) throw new NullReferenceException();
        foreach (var pair in asDict)
        {
            object value = pair.Value;
            if (SettingsCatalog.IntegerValues.Contains(pair.Key)) value = int.Parse(pair.Value);
            else if (SettingsCatalog.RealValues.Contains(pair.Key)) value = float.Parse(pair.Value);
            ProjectSettings.Instance[pair.Key] = value;
        }

        // Boot up components
        var unused0 = RedisDb;
        var unused1 = Auth;
        var unused2 = AppQueryFactory;
        var unused3 = HubStorageFactory;
        var unused4 = ChatAppFactory;
    }
}