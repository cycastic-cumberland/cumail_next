namespace AuthModule.Paralex;

public class ConcurrencyException : Exception
{
    public ConcurrencyException(string msg) : base(msg) {}
    public ConcurrencyException() {}
}