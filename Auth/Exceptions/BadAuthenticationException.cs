namespace smart_home_server.Auth.Exceptions;

public class BadAuthenticationException : Exception
{
    public BadAuthenticationException()
    {
    }

    public BadAuthenticationException(string message)
        : base(message)
    {
    }

    public BadAuthenticationException(string message, Exception inner)
        : base(message, inner)
    {
    }
}