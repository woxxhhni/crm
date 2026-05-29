using Asp.Versioning;
using AutoMapper;
using Cls.Api.Attributes;
using Cls.Api.Extensions;
using Cls.Api.Models;
using Cls.Api.Services;
using Cls.Application.Users.Commands;
using Cls.Application.Users.Queries;
using Cls.Domain.Enums;
using Cls.Shared.Contracts.Abstractions;
using Cls.Shared.Contracts.Common;
using Cls.Shared.Contracts.Users;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Cls.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ClsAuthorize]
public class EmployeesController(
    IMediator mediator,
    IMapper mapper,
    ICurrentUserService currentUserService,
    IFileService fileService) : ClsControllerBase
{
    [HttpGet]
    [ClsAuthorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<PagedResult<UserResponse>>> List(
        [FromQuery] string? role,
        [FromQuery] bool? isActive,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = null,
        CancellationToken ct = default)
    {
        var paging = new PagedRequest
        {
            Page = page,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDir = Enum.TryParse<SortDirection>(sortDir, true, out var direction) ? direction : null
        };

        var result = await mediator.Send(new ListUsersQuery(role, isActive, search, paging), ct);
        return Ok(new PagedResult<UserResponse>
        {
            Page = result.Page,
            PageSize = result.PageSize,
            Total = result.Total,
            Items = mapper.Map<IReadOnlyList<UserResponse>>(result.Items)
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserResponse>> Get(int id, CancellationToken ct)
    {
        var user = await mediator.Send(new GetEmployeeByIdQuery(id), ct);
        if (user is null)
            return NotFound();

        var response = mapper.Map<UserResponse>(user);
        if (response.UserProfileFileId.HasValue)
            response.UserProfileUrl = await fileService.GetFileUrl(response.UserProfileFileId.Value, ct);

        return Ok(response);
    }

    [HttpPost]
    [ClsAuthorize(Roles = "Admin,Manager")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<UserResponse>> Create(
        [FromForm] UserCreateRequest req,
        [FromForm] UploadSingleFileModel? file,
        CancellationToken ct)
    {
        var fileId = await UploadProfileFileAsync(file, ct);
        var created = await mediator.Send(new CreateEmployeeCommand(
            req.Name,
            req.Email,
            req.Password,
            req.Phone,
            req.Address,
            req.Description,
            req.Role,
            req.IsActive,
            fileId), ct);

        return CreatedAtAction(nameof(Get), new { id = created.Id }, mapper.Map<UserResponse>(created));
    }

    [HttpPut("{id:int}")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<UserResponse>> Update(
        int id,
        [FromForm] UserUpdateRequest req,
        [FromForm] UploadSingleFileModel? file,
        CancellationToken ct)
    {
        var fileId = await UploadProfileFileAsync(file, ct);
        var updated = await mediator.Send(new UpdateEmployeeCommand(
            id,
            req.Name,
            req.Email,
            req.Phone,
            req.Address,
            req.Description,
            req.Role,
            req.IsActive,
            fileId), ct);

        if (updated is null)
            return NotFound();

        return Ok(mapper.Map<UserResponse>(updated));
    }

    [HttpDelete("{id:int}")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> Delete(int id, CancellationToken ct)
    {
        var ok = await mediator.Send(new DeleteUserCommand(id), ct);
        return ok ? NoContent() : NotFound();
    }

    [HttpPut("{id:int}/reset-password")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> ResetPassword(int id, [FromBody] ResetPasswordRequest req, CancellationToken ct)
    {
        await mediator.Send(new ResetPasswordCommand(id, req.NewPassword, req.ConfirmPassword), ct);
        return NoContent();
    }

    private async Task<int?> UploadProfileFileAsync(UploadSingleFileModel? file, CancellationToken ct)
    {
        if (file?.File is null)
            return null;

        return await fileService.UploadFile(currentUserService.UserId, file.File, UploadFileBucket.EmployeeProfile, ct);
    }
}
