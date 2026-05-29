using System.Net;

namespace Cls.Shared.Exceptions;

public class NotFoundException : BusinessException
{
    public NotFoundException(string message = "Not found")
        : base(HttpStatusCode.NotFound, message)
    {
    }
}