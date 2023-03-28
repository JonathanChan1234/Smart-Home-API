
namespace smart_home_server.Exceptions;
class ForbiddenException : Exception
{
    public ForbiddenException()
    {
    }

    public ForbiddenException(string? message) : base(message)
    {
    }
}