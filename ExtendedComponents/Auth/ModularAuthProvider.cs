using Auth;

namespace ExtendedComponents.Auth;

public abstract class UserGenFactory
{
    public abstract AuthUserGenerator Create();
}

public abstract class TokenizerFactory
{
    public abstract AuthTokenProvider Create(string secret);
}

public abstract class AuthAgentFactory<TAuthAgent> where TAuthAgent : AuthAgent
{
    public abstract TAuthAgent Create(AuthQuery query, string tokenSecret);
}

public class ModularAuthProvider : TokenBasedAuthProvider<AuthAgent>
{
    private ModularAuthProvider(AuthAgent authAgent) : base(authAgent) {}

    public static ModularAuthProvider Create<TFactory>(AuthQuery query, string tokenSecret)
        where TFactory : AuthAgentFactory<AuthAgent>, new()
    {
        return new(new TFactory().Create(query, tokenSecret));
    }

    public static ModularAuthProvider Create<TUserGen, TTokenizer>(AuthQuery query, string tokenSecret)
        where TUserGen : UserGenFactory, new()
        where TTokenizer : TokenizerFactory, new()
    {
        return new(new DefaultAuthAgent(query, new TUserGen().Create(),
            new TTokenizer().Create(tokenSecret)));
    }
    

    public override void OnSignupSuccess(Func<AuthUser, SignupIntervention> action)
    {
        Agent.OnSignupSuccess(action);
    }
}