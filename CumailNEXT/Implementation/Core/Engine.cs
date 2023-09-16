using System.Reflection;
using Auth;
using CoreComponents.Core;
using CumailNEXT.Implementation.ChatApp;
using ExtendedComponents.Auth;
using ExtendedComponents.Core;
using Newtonsoft.Json;
using PostgresChatApp.ChatApp;

namespace CumailNEXT.Implementation.Core;

internal class JwtTokenizerFactory : TokenizerFactory
{
    public override AuthTokenProvider Create(string secret) => new JwtTokenProvider(secret);
}

internal class BCryptUserGenFactory : UserGenFactory
{
    public override AuthUserGenerator Create() => new BCryptUserGen();
}

public class Engine
{
    private static readonly object InstanceLock = new double();
    private static AuthProvider? _auth;
    private static PostgresChatAppQueryFactory? _appQueryFactory = null;
    private static QueuedChatHubStorage? _hubStorageFactory = null;
    private static MonolithicChatAppFactory? _chatAppFactory = null;
    private static byte[]? _tokenIV = null;
    public const string ConfigFileName = "config.json";

    public static PostgresChatAppQueryFactory AppQueryFactory => _appQueryFactory!;

    public static PostgresChatAppQuery NewAppQuery => AppQueryFactory.NewQueryInstance();
    public static AuthProvider Auth => _auth!;

    public static MonolithicChatAppFactory ChatAppFactory => _chatAppFactory!;

    public static QueuedChatHubStorage HubStorageFactory => _hubStorageFactory!;

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

        _appQueryFactory = PostgresChatApp.SelfInitializer.CreateQueryFactory();
        _auth = RedisAuth.SelfInitializer.Create(new PostgresChatApp.SelfInitializer(_appQueryFactory).Inject);
        _hubStorageFactory = new NotQueuedChatHubStorage();
        _chatAppFactory = new MonolithicChatAppFactory(Auth, AppQueryFactory);
    }
}