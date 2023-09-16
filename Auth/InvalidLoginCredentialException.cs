namespace Auth
{
    public class InvalidLoginCredentialException : Exception
    {
        public InvalidLoginCredentialException(string message = "") : base(message) { }
    }
}
