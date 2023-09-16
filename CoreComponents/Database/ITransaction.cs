using System.Data;

namespace CoreComponents.Database;

public interface ITransaction : IDisposable
{
    public void Start();
    public void RollBack();
    public void Commit();
    public IDbTransaction? GetRawTransaction();
}