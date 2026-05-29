using Asp.Versioning;
using Cls.Application.Abstractions;
using Cls.Application.Auth.Commands;
using Cls.Shared.Contracts.Users;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Cls.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController(IMediator mediator, IRecaptchaService recaptchaService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest req,
        [FromHeader(Name = "X-Recaptcha-Token")] string? recaptchaToken,
        CancellationToken ct)
    {
        if (!await recaptchaService.VerifyAsync(recaptchaToken))
            return BadRequest(new { error = "Recaptcha verification failed." });

        var token = await mediator.Send(new AuthenticateCommand(req.Email, req.Password), ct);
        return Ok(token);
    }
}
