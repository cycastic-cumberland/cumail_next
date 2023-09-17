using System.Reflection;
using Auth;
using ChatApp;
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
    private static AuthProvider? _auth;
    private static ChatAppQueryFactory? _appQueryFactory;
    private static QueuedChatHubStorage? _hubStorageFactory;
    private static MonolithicChatAppFactory? _chatAppFactory;
    private static byte[]? _tokenIV;
    public const string ConfigFileName = "config.json";

    public static ChatAppQueryFactory AppQueryFactory => _appQueryFactory!;

    public static ChatAppQuery NewAppQuery => AppQueryFactory.CreateInstance();
    public static AuthProvider Auth => _auth!;

    public static MonolithicChatAppFactory ChatAppFactory => _chatAppFactory!;

    public static QueuedChatHubStorage HubStorageFactory => _hubStorageFactory!;

    public static byte[] TokenInitVector => _tokenIV!;

    private static void OptionalDispose(object? something)
        => (something as IDisposable)?.Dispose();
    private static void TearDown(object? _, ConsoleCancelEventArgs args)
    {
        OptionalDispose(_auth);
        OptionalDispose(_appQueryFactory);
        OptionalDispose(_hubStorageFactory);
        OptionalDispose(_chatAppFactory);
        Console.WriteLine("Engine's instances torn down");
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

        var chatAppDbType = ProjectSettings.Instance.Get(SettingsCatalog.ChatHubDbType, "");
        _appQueryFactory = chatAppDbType switch
        {
            "postgres" => PostgresChatApp.SelfInitializer.CreateQueryFactory(),
            "mongodb" => MongoDChatApp.SelfInitializer.CreateQueryFactory(),
            _ => throw new Exception($"Unsupported ChatApp database type: {chatAppDbType}")
        };
        _auth = RedisAuth.SelfInitializer.Create(new ChatAppInjector(_appQueryFactory).Inject);
        _hubStorageFactory = new NotQueuedChatHubStorage();
        _chatAppFactory = new MonolithicChatAppFactory(Auth, AppQueryFactory);
        _tokenIV = Crypto.GenerateSecureBytes(16);
        Console.CancelKeyPress += TearDown;
    }
}