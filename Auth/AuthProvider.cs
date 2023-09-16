namespace Auth;

public class UserIdQuery
{
    private readonly AuthQuery _query;
    public UserIdQuery(AuthQuery query)
    {
        this._query = query;
    }

    public AuthUser this[string key]
    {
        get => _query.GetUserById(key);
        set => _query.SetUserById(key, value);
    }
}
public class UserUuidQuery
{
    private readonly AuthQuery _query;
    public UserUuidQuery(AuthQuery query)
    {
        this._query = query;
    }

    public AuthUser this[string key] => _query.GetUserByUuid(key);
}

public abstract class AuthQuery : IDisposable
{
    public const int Timeout = 3000;
    protected readonly ReaderWriterLock RwLock;
    public readonly UserIdQuery ById;
    public readonly UserUuidQuery ByUuid;

    public virtual void Dispose() {}
    public AuthQuery()
    {
        ById = new UserIdQuery(this);
        ByUuid = new UserUuidQuery(this);
        RwLock = new ReaderWriterLock();
    }
    public abstract bool Has(string username);
    
    // public AuthUser this[string key]
    // {
    //     get => GetUserById(key);
    //     set => SetUserById(key, value);
    // }

    public virtual T Read<T>(Func<AuthQuery, T> action)
    {
        RwLock.AcquireReaderLock(Timeout);
        try
        {
            T re = action(this);
            RwLock.ReleaseReaderLock();
            return re;
        }
        catch (Exception)
        {
            RwLock.ReleaseReaderLock();
            throw;
        }
    }

    public virtual T Write<T>(Func<AuthQuery, T> action)
    {
        RwLock.AcquireWriterLock(Timeout);
        try
        {
            T re = action(this);
            RwLock.ReleaseWriterLock();
            return re;
        }
        catch (Exception)
        {
            RwLock.ReleaseWriterLock();
            throw;
        }
    }
    public abstract AuthUser GetUserById(string user);
    public abstract AuthUser GetUserByUuid(string uuid);
    public abstract void SetUserById(string userId, AuthUser user);
}

public abstract class AuthProvider : IDisposable
{
    protected Func<AuthUser, SignupIntervention>? SignupSuccessAction;
    public enum SignupIntervention
    {
        Allow,
        Reject
    }
    public virtual void Dispose(){}
    public abstract AuthToken SignupWithEmailPassword(AuthRequest request);
    public abstract AuthToken LoginWithEmailPassword(AuthRequest request);
    public abstract AuthToken RefreshAuthToken(AuthToken refreshToken);
    public abstract AuthUser GetUserByIdToken(AuthToken token);
    public abstract AuthUser GetUserByUUID(string uuid);
    public abstract bool IsAuthTokenValid(AuthToken token);
    public abstract string GetUserIdFromToken(AuthToken token);
    public virtual void OnSignupSuccess(Func<AuthUser, SignupIntervention> action)
    {
        SignupSuccessAction = action;
    }
}
