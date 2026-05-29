namespace Cls.Application.Abstractions;

public interface IRecaptchaService
{
    Task<bool> VerifyAsync(string? token);
}
