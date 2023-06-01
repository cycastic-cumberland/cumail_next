namespace CumailNEXT.Components.Auth;

public class MalformedEmailException : Exception
{
    public MalformedEmailException(string message = "") : base(message) { }
}