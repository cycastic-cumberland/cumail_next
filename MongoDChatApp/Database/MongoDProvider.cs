using System.Data;
using CoreComponents.Database;
using ExtendedComponents.Core;
using ExtendedComponents.Exceptions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoDChatApp.Database;

public class MongoDTransaction : ITransaction
{
    private readonly IClientSessionHandle _session;

    public MongoDTransaction(IClientSessionHandle session)
    {
        _session = session;
    }
    
    public void Dispose()
    {
        _session.Dispose();
    }

    public void Start()
    {
        _session.StartTransaction();
    }

    public void RollBack()
    {
        _session.AbortTransaction();
    }

    public void Commit()
    {
        _session.CommitTransaction();
    }

    public IDbTransaction? GetRawTransaction()
    {
        return null;
    }
}

public class MongoDConnectionSettings
{
    public string Address { get; set; } = "localhost";
    public int Port { get; set; } = 27017;
    public string DatabaseName { get; set; } = "";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";

    public MongoClientSettings ToSettings()
    {
        return new()
        {
            Credential = MongoCredential.CreateCredential(DatabaseName, Username, Password),
            Server = new MongoServerAddress(Address, Port)
        };
    }
}

public class MongoDProvider
{
    private readonly int _reconnectionTime;
    private MongoClient? _client;
    private IMongoDatabase? _db;
    private MongoDConnectionSettings? _lastConnection;
    
    public MongoDProvider()
    {
        _reconnectionTime = ProjectSettings.Instance.Get(SettingsCatalog.CoreDbMaxReconnectionTimeMs, 500);
    }

    public MongoDProvider(MongoDConnectionSettings settings) : this()
    {
        Connect(settings);
    }

    public void Connect(MongoDConnectionSettings settings)
    {
        _lastConnection = settings;
        var epoch = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        var retryCount = 0;
        while (DateTimeOffset.Now.ToUnixTimeMilliseconds() - epoch < _reconnectionTime)
        {
            try
            {
                Reconnect();
                return;
            }
            catch (TimeoutException)
            {
                Thread.Sleep(100);
                retryCount++;
            }
        }

        throw new ConnectionTimeoutException($"Failed to establish a connection after {retryCount} attempts.");
    }
    
    public void Reconnect()
    {
        if (_lastConnection == null) return;
        _db = null;
        _client = new(_lastConnection.ToSettings());
        _db = _client.GetDatabase(_lastConnection.DatabaseName);
    }

    public bool IsConnected()
    {
        if (_db == null) return false;
        try
        {
            // Execute a simple command to check connectivity
            var command = new BsonDocument("ping", 1);
            _db.RunCommand<BsonDocument>(command);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public IMongoCollection<T> GetCollection<T>(string name, MongoCollectionSettings? settings = null)
    {
        if (_db == null) throw new DbConnectionFailedException();
        return _db.GetCollection<T>(name, settings);
    }
    
    public void CreateCollection(string name, CreateCollectionOptions? settings = null, CancellationToken token = default)
    {
        if (_db == null) throw new DbConnectionFailedException();
        _db.CreateCollection(name, settings, token);
    }

    public MongoDTransaction CreateTransaction()
    {
        if (_client == null) throw new DbConnectionFailedException();
        return new(_client.StartSession());
    }
    
    public void Disconnect()
    {
        _client = null;
        _db = null;
    }
}