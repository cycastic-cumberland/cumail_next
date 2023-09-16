using Auth;
using ExtendedComponents.Auth;
using ExtendedComponents.Core;
using RedisAuth.Auth;
using RedisAuth.Database;

namespace RedisAuth;

internal class JwtTokenizerFactory : TokenizerFactory
{
    public override AuthTokenProvider Create(string secret) => new JwtTokenProvider(secret);
}

internal class BCryptUserGenFactory : UserGenFactory
{
    public override AuthUserGenerator Create() => new BCryptUserGen();
}


public static class SelfInitializer
{
    private static RedisProvider CreateDbProvider()
    {
        return new (new RedisConnectionConfig
        {
            Address = ProjectSettings.Instance.Get(SettingsCatalog.AuthDbEndpoint, ""),
            Username = ProjectSettings.Instance.Get(SettingsCatalog.AuthDbUsername, ""),
            Password = ProjectSettings.Instance.Get(SettingsCatalog.AuthDbPassword, "")
        });
    }
    public static AuthProvider Create(Func<AuthUser, AuthProvider.SignupIntervention> injection)
    {
        var authQuery = new MonolithicRedisAuthQuery(CreateDbProvider());
        var secret = ProjectSettings.Instance.Get(SettingsCatalog.AuthTokenSecret, "");
        if (secret == "")
            throw new Exception("Token secret not found");
        var auth = ModularAuthProvider.Create<BCryptUserGenFactory, JwtTokenizerFactory>(authQuery, secret);
        auth.OnSignupSuccess(injection);
        return auth;
    }
}