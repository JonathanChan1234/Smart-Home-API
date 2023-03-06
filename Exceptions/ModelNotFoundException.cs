namespace smart_home_server.Exceptions;

class ModelNotFoundException : Exception
{
    public ModelNotFoundException()
    {

    }
    public ModelNotFoundException(string? message) : base(message)
    {

    }
}