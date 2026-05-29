using System.Net;

namespace Cls.Shared.Exceptions;

public class ForbiddenException : BusinessException
{
    public ForbiddenException(string message = "You have not permission.")
        : base(HttpStatusCode.Forbidden, message)
    {
    }
}