namespace CumailNEXT.Components.Auth;

public class InvalidPasswordException : Exception
{
    public InvalidPasswordException(string message = "") : base(message) { }
}