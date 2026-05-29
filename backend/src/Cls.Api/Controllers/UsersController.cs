using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Cls.Shared.Contracts.Users;
using Cls.Shared.Contracts.Common;
using Cls.Application.Users.Queries;
using Cls.Application.Users.Commands;
using Asp.Versioning;
using Cls.Api.Attributes;
using Cls.Api.Extensions;

namespace Cls.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class UsersController(IMediator mediator, IMapper mapper) : ClsControllerBase
{
    [HttpGet]
    [ClsAuthorize]
    public async Task<ActionResult<PagedResult<UserResponse>>> List(
        [FromQuery] string? role, [FromQuery] bool? isActive, [FromQuery] string? search,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? sortBy = null, [FromQuery] string? sortDir = null)
    {
        var req = new PagedRequest { Page = page, PageSize = pageSize, SortBy = sortBy, SortDir = Enum.TryParse<SortDirection>(sortDir, true, out var d) ? d : null };
        var result = await mediator.Send(new ListUsersQuery(role, isActive, search, req));
        return Ok(new PagedResult<UserResponse> { Page = result.Page, PageSize = result.PageSize, Total = result.Total, Items = mapper.Map<IReadOnlyList<UserResponse>>(result.Items) });
    }

    [HttpGet("{id:int}")]
    [ClsAuthorize]
    public async Task<ActionResult<UserResponse>> Get(int id)
    {
        var user = await mediator.Send(new GetUserByIdQuery(id));
        if (user is null) return NotFound();
        return Ok(mapper.Map<UserResponse>(user));
    }

    [HttpPost]
    [ClsAuthorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<UserResponse>> Create([FromBody] UserCreateRequest req)
    {
        if (!Enum.TryParse<UserRole>(req.Role, true, out var role)) role = UserRole.Employee;
        var created = await mediator.Send(new CreateUserCommand(req.Name, req.Email, req.Password, req.Phone, req.Address, req.Description, role, req.IsActive));
        var dto = mapper.Map<UserResponse>(created);
        return CreatedAtAction(nameof(Get), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:int}")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<UserResponse>> Update(int id, [FromBody] UserUpdateRequest req)
    {
        if (!Enum.TryParse<UserRole>(req.Role, true, out var role)) role = UserRole.Employee;
        var updated = await mediator.Send(new UpdateUserCommand(id, req.Name, req.Email, req.Phone, req.Address, req.Description, role, req.IsActive));
        if (updated is null) return NotFound();
        return Ok(mapper.Map<UserResponse>(updated));
    }

    [HttpDelete("{id:int}")]
    [ClsAuthorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> Delete(int id)
    {
        var ok = await mediator.Send(new DeleteUserCommand(id));
        return ok ? NoContent() : NotFound();
    }
}
