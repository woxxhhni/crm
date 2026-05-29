using System.Net;

namespace Cls.Shared.Exceptions;

public class InvalidActionException : BusinessException
{
    public InvalidActionException(string message = "Invalid operation")
        : base(HttpStatusCode.BadRequest, message)
    {
    }
}