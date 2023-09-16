namespace Auth;

public class NoMatchingUUIDFoundException : Exception
{
    public NoMatchingUUIDFoundException(string message = "") : base(message) { }
}