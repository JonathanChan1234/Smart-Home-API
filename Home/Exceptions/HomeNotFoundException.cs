using System.Runtime.Serialization;

namespace smart_home_server.Home.Exceptions;
[Serializable]
internal class HomeNotFoundException : Exception
{
    public HomeNotFoundException()
    {
    }

    public HomeNotFoundException(string? message) : base(message)
    {
    }

    public HomeNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected HomeNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}