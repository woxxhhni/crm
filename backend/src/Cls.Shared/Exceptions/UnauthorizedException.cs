using System.Net;

namespace Cls.Shared.Exceptions;

public class UnauthorizedException : BusinessException
{
    public UnauthorizedException(string message = "User disabled.")
        : base(HttpStatusCode.Unauthorized, message)
    {
    }
}