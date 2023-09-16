namespace ExtendedComponents.Core;

public static class SettingsCatalog
{
    public const string AuthPasswordMinimumLength = "auth/password/min_len";
    public const string AuthPasswordSaltRound = "auth/password/salt_round";
    public const string AuthDbEndpoint = "auth/db/endpoint";
    public const string AuthDbUsername = "auth/db/username";
    public const string AuthDbPassword = "auth/db/password";
    public const string AuthTokenSecret = "auth/token/secret";
    public const string AuthTokenExpirationHour = "auth/token/expiration";
    public const string AuthIssuer = "auth/issuer";
    public const string ChatHubConnectionAddress = "chat/connection/address";
    public const string ChatHubConnectionDatabaseName = "chat/connection/dbname";
    public const string ChatHubConnectionPort = "chat/connection/port";
    public const string ChatHubConnectionUsername = "chat/connection/username";
    public const string ChatHubConnectionPassword = "chat/connection/password";
    public const string ChatRoomNameMaxLength = "chat/room_max_length";
    
    public static readonly string[] IntegerValues = { AuthPasswordMinimumLength, AuthPasswordSaltRound, ChatHubConnectionPort, ChatRoomNameMaxLength };
    public static readonly string[] RealValues = { AuthTokenExpirationHour };
}

public class ProjectSettings
{
    private readonly IDictionary<string, object> _configs;
    private static ProjectSettings? _instance = null;

    public static ProjectSettings Instance => _instance ??= new ProjectSettings();

    public ProjectSettings()
    {
        _configs = new Dictionary<string, object>(30);
    }

    public void Set(string key, object? value)
    {
        _configs[key] = value ?? throw new NullReferenceException();
    }

    public T Get<T>(string key, T defaultValue)
    {
        try
        {
            var raw = _configs[key];
            return raw is T raw1 ? raw1 : defaultValue;
        }
        catch (KeyNotFoundException)
        {
            return defaultValue;
        }
    }

    public object this[string key]
    {
        set => Set(key, value);
    }
}