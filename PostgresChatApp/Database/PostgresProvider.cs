using System.Data;
using System.Reflection;
using System.Text;
using CoreComponents.Database;
using Dapper;
using ExtendedComponents.Core;
using ExtendedComponents.Exceptions;
using Npgsql;


namespace PostgresChatApp.Database;

public class PostgresTransaction : ITransaction
{
    private readonly NpgsqlTransaction _realTransaction;
    
    public PostgresTransaction(NpgsqlTransaction trans)
    {
        _realTransaction = trans;
    }
    public void Dispose()
    {
        _realTransaction.Dispose();
    }
    public void Start()
    {
        
    }
    public void RollBack()
    {
        _realTransaction.Rollback();
    }

    public void Commit()
    {
        _realTransaction.Commit();
    }

    public IDbTransaction GetRawTransaction() => _realTransaction;
}

public class PostgresConnectionSettings
{
    public string Address { get; set; } = "localhost";
    public int Port { get; set; } = 5432;
    public string DatabaseName { get; set; } = "";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";

    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append($"Server={Address};Port={Port};")
               .Append($"Database={DatabaseName};Username={Username};");
        if (Password.Length != 0)
            builder.Append($"Password='{Password}';");
        return builder.ToString();
    }
}

public class PostgresProvider : IDisposable
{
    private readonly int _reconnectionTime;
    private NpgsqlConnection? _conn;
    private PostgresConnectionSettings? _lastConnection;

    public PostgresProvider()
    {
        _reconnectionTime = ProjectSettings.Instance.Get(SettingsCatalog.CoreDbMaxReconnectionTimeMs, 500);
    }

    public PostgresProvider(PostgresConnectionSettings settings) : this()
    {
        Connect(settings);
    }

    public void Connect(PostgresConnectionSettings settings)
        => ConnectAsync(settings).Wait();
    
    public async Task ConnectAsync(PostgresConnectionSettings settings)
    {
        if (_conn != null) await _conn.DisposeAsync();
        _lastConnection = settings;
        // _conn = new NpgsqlConnection(_lastConnection.ToString());
        var epoch = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        var retryCount = 0;
        while (DateTimeOffset.Now.ToUnixTimeMilliseconds() - epoch < _reconnectionTime)
        {
            try
            {
                await ReconnectAsync();
                return;
            }
            catch (NpgsqlException ex)
            {
                if (ex.ErrorCode != -2147467259)
                    throw;
                await Task.Delay(100);
                retryCount++;
            }
        }

        throw new ConnectionTimeoutException($"Failed to establish a connection after {retryCount} attempts.");
    }

    public void Reconnect()
        => ReconnectAsync().Wait();
    
    public async Task ReconnectAsync()
    {
        if (_lastConnection == null) return;
        if (_conn != null) await _conn.DisposeAsync();
        _conn = new NpgsqlConnection(_lastConnection.ToString());
        await _conn.OpenAsync();
    }

    public bool IsConnected()
    {
        if (_conn == null) return false;
        return _conn.State == ConnectionState.Open;
    }

    public IEnumerable<T> MappedQuery<T>(string sql, object? param = null, IDbTransaction? transaction = null)
    {
        return _conn.Query<T>(sql, param: param, transaction: transaction);
    }
    
    public async Task<IEnumerable<T>> MappedQueryAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null)
    {
        return await _conn.QueryAsync<T>(sql, param: param, transaction: transaction);
    }
    
    public int Execute(string sql, object? param = null, IDbTransaction? transaction = null)
    {
        return _conn.Execute(sql, param: param, transaction: transaction);
    }
    
    public async Task<int> ExecuteAsync(string sql, object? param = null, IDbTransaction? transaction = null)
    {
        return await _conn.ExecuteAsync(sql, param: param, transaction: transaction);
    }

    public IEnumerable<T> TryMappedQuery<T>(string sql, object? param = null, IDbTransaction? transaction = null)
    {
        var task = TryMappedQueryAsync<T>(sql, param, transaction);
        task.Wait();
        if (task.Exception != null) throw task.Exception;
        return task.Result;
    }

    public int TryExecute(string sql, object? param = null, IDbTransaction? transaction = null)
    {
        var task = TryExecuteAsync(sql, param, transaction);
        task.Wait();
        if (task.Exception != null) throw task.Exception;
        return task.Result;
    }
    
    public async Task<IEnumerable<T>> TryMappedQueryAsync<T>(string sql, object? param = null,
        IDbTransaction? transaction = null)
    {
        var epoch = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        var retryCount = 0;
        while (DateTimeOffset.Now.ToUnixTimeMilliseconds() - epoch < _reconnectionTime)
        {
            try
            {
                return await MappedQueryAsync<T>(sql, param, transaction);
            }
            catch (NpgsqlException ex)
            {
                if (ex.ErrorCode != -2147467259)
                {
                    await Task.Delay(100);
                    try
                    {
                        await ReconnectAsync();
                    }
                    catch (Exception)
                    {
                        retryCount++;
                    }
                }
                else throw;
            }
        }
        throw new ConnectionTimeoutException($"Failed to establish a connection after {retryCount} attempts.");
    }

    public async Task<int> TryExecuteAsync(string sql, object? param = null, IDbTransaction? transaction = null)
    {
        var epoch = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        var retryCount = 0;
        while (DateTimeOffset.Now.ToUnixTimeMilliseconds() - epoch < _reconnectionTime)
        {
            try
            {
                return await ExecuteAsync(sql, param, transaction);
            }
            catch (NpgsqlException ex)
            {
                if (ex.ErrorCode != -2147467259)
                {
                    await Task.Delay(100);
                    try
                    {
                        await ReconnectAsync();
                    }
                    catch (Exception)
                    {
                        retryCount++;
                    }
                }
                else throw;
            }
        }
        throw new ConnectionTimeoutException($"Failed to establish a connection after {retryCount} attempts.");
    }

    public PostgresTransaction CreateTransaction()
    {
        if (_conn == null) throw new DbConnectionFailedException();
        return new PostgresTransaction(_conn.BeginTransaction());
    }
    public void Disconnect()
    {
        _conn?.Dispose();
    }
    
    public void Dispose()
    {
        Disconnect();
    }
}