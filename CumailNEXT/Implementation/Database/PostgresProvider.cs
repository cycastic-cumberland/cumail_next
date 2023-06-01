using System.Data;
using System.Reflection;
using System.Text;
using CumailNEXT.Components.Database;
using Dapper;
using Npgsql;
// using Pipelines.Sockets.Unofficial;

namespace CumailNEXT.Implementation.Database;

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
    private NpgsqlConnection? _conn = null;
    // public NpgsqlConnection RawConnection => conn;

    public PostgresProvider()
    {
        
    }

    public PostgresProvider(PostgresConnectionSettings settings)
    {
        Connect(settings);
    }

    public void Connect(PostgresConnectionSettings settings)
    {
        _conn?.Dispose();
        _conn = new NpgsqlConnection(settings.ToString());
        _conn.Open();
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

    public NpgsqlDataReader RawQuery(string sql, object? param = null)
    {
        var command = new NpgsqlCommand(sql, _conn);
        if (param == null) return command.ExecuteReader();
        var properties = param.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in properties)
        {
            var value = prop.GetValue(param);
            command.Parameters.AddWithValue($"@{prop.Name}", value ?? DBNull.Value);
        }

        return command.ExecuteReader();
    }
    public int Execute(string sql, object? param = null, IDbTransaction? transaction = null)
    {
        return _conn.Execute(sql, param: param, transaction: transaction);
    }

    public PostgresTransaction CreateTransaction()
    {
        if (!IsConnected() || _conn == null) throw new DbConnectionFailedException();
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