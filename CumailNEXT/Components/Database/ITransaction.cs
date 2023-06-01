using System.Data;

namespace CumailNEXT.Components.Database;

public interface ITransaction : IDisposable
{
    public void Start();
    public void RollBack();
    public void Commit();
    public IDbTransaction? GetRawTransaction();
}