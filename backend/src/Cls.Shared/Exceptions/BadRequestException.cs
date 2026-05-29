using System.Net;

namespace Cls.Shared.Exceptions;

public class BadRequestException : BusinessException
{
    public BadRequestException(string message = "Bad request")
        : base(HttpStatusCode.BadRequest, message)
    {
    }
}