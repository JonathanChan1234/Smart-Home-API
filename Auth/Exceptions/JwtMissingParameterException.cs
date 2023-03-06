namespace smart_home_server.Auth.Exceptions;
class JwtMissingParameterException : Exception
{
    public JwtMissingParameterException()
    {
    }

    public JwtMissingParameterException(string message)
        : base(message)
    {
    }

    public JwtMissingParameterException(string message, Exception inner)
        : base(message, inner)
    {
    }
}