using Cls.Domain.Entities;
namespace Cls.Application.Abstractions;
public interface IJwtTokenService
{
    string Generate(User user, out DateTime expiresIn);
}
