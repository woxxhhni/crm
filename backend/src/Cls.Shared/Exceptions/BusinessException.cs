using System.Net;

namespace Cls.Shared.Exceptions;

public class BusinessException : Exception
{
    public HttpStatusCode Code { get; }

    public BusinessException(string message)
        : base(message)
    {
        Code = HttpStatusCode.InternalServerError;
    }

    public BusinessException(HttpStatusCode code, string message)
        : base(message)
    {
        Code = code;
    }
}
