namespace Auth;

public abstract class AuthAgent : AuthProvider
{
    protected readonly AuthQuery Query;
    protected readonly AuthUserGenerator UserGen;
    protected readonly AuthTokenProvider Tokenizer;
    public AuthAgent(AuthQuery query, AuthUserGenerator userGen, AuthTokenProvider tokenizer)
    {
        Query = query;
        UserGen = userGen;
        Tokenizer = tokenizer;
    }

    public override void Dispose()
    {
        Tokenizer.Dispose();
    }
}