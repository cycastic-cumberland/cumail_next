namespace ExtendedComponents.Exceptions;

public class ConnectionTimeoutException : Exception
{
    public ConnectionTimeoutException() : base() {}
    public ConnectionTimeoutException(string msg) : base(msg) {}
}