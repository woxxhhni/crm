using System.Net;

namespace Cls.Shared.Exceptions;

public class InvalidAccessException : BusinessException
{
    public InvalidAccessException(string message = "Invalid access")
        : base(HttpStatusCode.Forbidden, message)
    {
    }
}