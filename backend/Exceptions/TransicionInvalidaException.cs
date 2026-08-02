namespace backend.Exceptions;

public class TransicionInvalidaException : Exception
{
    public TransicionInvalidaException(string message)
        : base(message)
    {
    }
}