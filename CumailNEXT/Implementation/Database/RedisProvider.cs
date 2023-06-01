using System.Data;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace CumailNEXT.Implementation.Database;

public class RedisTransaction : CumailNEXT.Components.Database.ITransaction
{
    public readonly ITransaction RealTransaction;
    public bool Finished { get; private set; } = false;
    public RedisTransaction(ITransaction trans)
    {
        RealTransaction = trans;
    }
    public void Dispose()
    {
        
    }

    public void Start()
    {
        
    }

    public void RollBack()
    {
        
    }

    public void Commit()
    {
        RealTransaction.ExecuteAsync().Wait();
        Finished = true;
    }

    public IDbTransaction? GetRawTransaction() => null;
}

public class RedisConnectionConfig
{
    public string Address = "";
    public string Username = "";
    public string Password = "";
}

public class RedisProvider : IDisposable
{
    public bool AutoReconnect = true;
    private readonly Dictionary<int, RedisTransaction> _transactionsCatalog = new();
    private ConnectionMultiplexer? _redis;
    private IDatabase? _db;
    private ConfigurationOptions? _lastConnectionConfig;

    private RedisTransaction? GetTransaction()
    {
        var id = Environment.CurrentManagedThreadId;
        if (!_transactionsCatalog.ContainsKey(id)) return null;
        return _transactionsCatalog[id];
    }

    private void SetTransaction(RedisTransaction trans)
    {
        var id = Environment.CurrentManagedThreadId;
        _transactionsCatalog[id] = trans;
    }
    
    public RedisProvider(RedisConnectionConfig connectionConfig)
    {
        Connect(connectionConfig);
    }

    public RedisProvider()
    {
        
    }

    public bool IsConnected() =>  _redis?.IsConnected ?? false;

    public bool Connect(RedisConnectionConfig connectionConfig)
    {
        _lastConnectionConfig = new ConfigurationOptions
        {
            AbortOnConnectFail = false,
            EndPoints = { connectionConfig.Address },
            Password = connectionConfig.Password,
        };
        if (!connectionConfig.Username.Equals(""))
            _lastConnectionConfig.User = connectionConfig.Username;
        return Reconnect();
    }
    
    public bool Reconnect()
    {
        if (_lastConnectionConfig == null) return false;
        _redis = ConnectionMultiplexer.Connect(_lastConnectionConfig);
        if (!_redis.IsConnected) return false;
        _db = _redis.GetDatabase();
        return true;
    }

    public void Disconnect()
    {
        _db = null;
        _redis = null;
    }

    public void PutManual(string key, string value)
    {
        var db = _db;
        if (db == null) throw new RedisConnectionFailed();
        var trans = GetTransaction();
        if (trans is { Finished: false })
            trans.RealTransaction.StringSetAsync(key, value);
        else
            db.StringSet(key, value);
    }

    public void Put(string key, object value)
    {
        if (value is string s) PutManual(key, s);
        else if (value is String ss) PutManual(key, ss);
        else PutManual(key, JsonConvert.SerializeObject(value));
    }

    public bool PutSafe(string key, object value)
    {
        try
        {
            Put(key, value);
            return true;
        }
        catch (Exception)
        {
            // Ignored
        }

        if (!AutoReconnect) return false;
        try
        {
            Reconnect();
            Put(key, value);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public string GetManual(string key)
    {
        var db = _db;
        if (db == null) throw new RedisConnectionFailed();
        RedisValue raw = db.StringGet(key);
        if (raw.IsNull) throw new RedisKeyNotFoundException();
        return raw.ToString();
    }

    public T Get<T>(string key)
    {
        string raw = GetManual(key);
        var re = JsonConvert.DeserializeObject<T>(raw);
        if (re == null) throw new Newtonsoft.Json.JsonException();
        return re;
    }

    public T? GetSafe<T>(string key)
    {
        try
        {
            return Get<T>(key);
        }
        catch (Exception)
        {
            //Ignored
        }
        if (!AutoReconnect) return default;
        try
        {
            Reconnect();
            return Get<T>(key);
        }
        catch (Exception)
        {
            return default;
        }
    }

    public bool Erase(string key)
    {
        var db = _db;
        if (db == null) throw new RedisConnectionFailed();
        return db.KeyDelete(key);
    }

    public bool Has(string key)
    {
        var db = _db;
        if (db == null) throw new RedisConnectionFailed();
        return db.KeyExists(key);
    }

    public RedisTransaction CreateTransaction()
    {
        lock (this)
        {
            if (_db == null) throw new RedisConnectionFailed();
            var trans = GetTransaction();
            if (trans is { Finished: false }) return trans;
            trans = new RedisTransaction(_db.CreateTransaction());
            SetTransaction(trans);

            return trans;
        }
    }

    public void Dispose()
    {
        _redis?.Dispose();
    }
}