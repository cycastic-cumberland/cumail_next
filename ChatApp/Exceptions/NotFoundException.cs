namespace ChatApp.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string notFoundWhat = "") : base(notFoundWhat)
    {
    }
}