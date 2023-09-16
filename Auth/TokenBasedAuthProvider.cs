namespace Auth;

public class TokenBasedAuthProvider<TAuthAgent> : AuthProvider
    where TAuthAgent : AuthAgent
{
    protected readonly TAuthAgent Agent;
        
    public TokenBasedAuthProvider(TAuthAgent agent)
    {
        Agent = agent;
    }

    public override void Dispose()
    {
        Agent.Dispose();
    }

    public override AuthToken SignupWithEmailPassword(AuthRequest request)
    {
        return Agent.SignupWithEmailPassword(request);
    }

    public override AuthToken LoginWithEmailPassword(AuthRequest request)
    {
        return Agent.LoginWithEmailPassword(request);
    }

    public override AuthToken RefreshAuthToken(AuthToken refreshToken)
    {
        return Agent.RefreshAuthToken(refreshToken);
    }

    public override AuthUser GetUserByIdToken(AuthToken token)
    {
        return Agent.GetUserByIdToken(token);
    }

    public override AuthUser GetUserByUUID(string uuid)
    {
        return Agent.GetUserByUUID(uuid);
    }

    public override bool IsAuthTokenValid(AuthToken token)
    {
        return Agent.IsAuthTokenValid(token);
    }

    public override string GetUserIdFromToken(AuthToken token)
    {
        return Agent.GetUserIdFromToken(token);
    }
}