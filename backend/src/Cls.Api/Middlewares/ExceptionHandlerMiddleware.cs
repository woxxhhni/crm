using System.Net;
using System.Text.Json;
using Cls.Shared.Exceptions;
using FluentValidation;

namespace Cls.Api.Middlewares;

public class ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            while (ex.InnerException != null)
                ex = ex.InnerException;

            var response = context.Response;
            response.ContentType = "application/json";
            var errorMessage = ex.Message;

            switch (ex)
            {
                case BusinessException e:
                    response.StatusCode = (int)e.Code;
                    break;
                case ValidationException ve:
                    response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                    errorMessage = string.Join(", ", ve.Errors.Select(e => e.ErrorMessage));
                    break;
                default:
                    logger.LogError(ex, errorMessage);
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorMessage = "An unexpected error occurred.";
                    break;
            }

            var newResp = new { status = response.StatusCode, detail = errorMessage };
            await response.WriteAsync(JsonSerializer.Serialize(newResp));
        }
    }
}
