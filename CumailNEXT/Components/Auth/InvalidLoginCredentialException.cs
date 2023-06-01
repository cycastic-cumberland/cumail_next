namespace Cumail_REST_API.Components.Auth
{
    public class InvalidLoginCredentialException : Exception
    {
        public InvalidLoginCredentialException(string message = "") : base(message) { }
    }
}
