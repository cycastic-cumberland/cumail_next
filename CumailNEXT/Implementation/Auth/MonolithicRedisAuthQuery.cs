using CumailNEXT.Components.Auth;
using CumailNEXT.Implementation.Database;

namespace CumailNEXT.Implementation.Auth;

public class MonolithicRedisAuthQuery : AuthQuery
{
    private readonly RedisProvider _mainDb;
    public MonolithicRedisAuthQuery(RedisProvider mainDb)
    {
        _mainDb = mainDb;
    }
    public override bool Has(string username)
    {
        return _mainDb.Has(username);
    }

    public override AuthUser GetUserById(string user)
    {
        var reference = _mainDb.Get<UuidReference>(user);
        return GetUserByUuid(reference.UserUuid);
    }

    public override AuthUser GetUserByUuid(string uuid)
    {
        return _mainDb.Get<AuthUser>(uuid);
    }

    public override void SetUserById(string userId, AuthUser user)
    {
        using var transaction = _mainDb.CreateTransaction();
        _mainDb.Put(user.UserUuid, user);
        _mainDb.Put(userId, new UuidReference
        {
            UserUuid = user.UserUuid
        });
        transaction.Commit();
    }
}